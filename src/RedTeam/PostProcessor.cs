﻿using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RedTeam
{
    public class PostProcessor
    {
        private GraphicsDevice _gfx;
        private SpriteBatch _batch;
        private RenderTarget2D _effectBuffer1;
        private RenderTarget2D _effectBuffer2;
        private RenderTarget2D _intermediate;
        private Effect _brightnessThreshold;
        private Effect _gaussian;
        private const int KERNEL_SIZE = 15;
        private Effect _bloom;
        private float _baseIntensity = 1;
        private float _baseSaturation = 1;
        private float _bloomIntensity = 0.4f;
        private float _bloomSaturation = 1;
        private float _bloomThreshold = 0.26f;
        private float _blurAmount = 1.1f;
        private Effect _shadowmask;
        private float _hardPix = -6;
        private float _hardScan = -10;
        
        public bool EnableShadowMask { get; set; } = true;
        public bool EnableBloom { get; set; } = true;
        
        private float[] _gaussianKernel = new float[KERNEL_SIZE]
        {
            0,
            0,
            0.000003f,
            0.000229f,
            0.005977f,
            0.060598f,
            0.24173f,
            0.382925f,
            0.24173f,
            0.060598f,
            0.005977f,
            0.000229f,
            0.000003f,
            0,
            0
        };

        private Vector2[] _offsets = new Vector2[KERNEL_SIZE];
        
        public PostProcessor(GraphicsDevice gfx)
        {
            _gfx = gfx;
            _batch = new SpriteBatch(_gfx);
        }

        public void LoadContent(ContentManager content)
        {
            _brightnessThreshold = content.Load<Effect>("Effects/BrightnessThreshold");
            _gaussian = content.Load<Effect>("Effects/Gaussian");
            _bloom = content.Load<Effect>("Effects/Bloom");
            _shadowmask = content.Load<Effect>("Effects/ShadowMask");
            
            _brightnessThreshold.Parameters["Threshold"].SetValue(_bloomThreshold);
            _gaussian.Parameters["Kernel"].SetValue(_gaussianKernel);

            _bloom.Parameters["BaseIntensity"].SetValue(_baseIntensity);
            _bloom.Parameters["BloomIntensity"].SetValue(_bloomIntensity);

            _bloom.Parameters["BloomSaturation"].SetValue(_bloomSaturation);
            _bloom.Parameters["BaseSaturation"].SetValue(_baseSaturation);
            
        }
        
        public void ReallocateEffectBuffers()
        {
            _effectBuffer1?.Dispose();
            _effectBuffer2?.Dispose();
            _intermediate?.Dispose();
            
            _effectBuffer1 = new RenderTarget2D(_gfx, _gfx.PresentationParameters.BackBufferWidth,
                _gfx.PresentationParameters.BackBufferHeight);
            _effectBuffer2 = new RenderTarget2D(_gfx, _gfx.PresentationParameters.BackBufferWidth,
                _gfx.PresentationParameters.BackBufferHeight);
            _intermediate = new RenderTarget2D(_gfx, _gfx.PresentationParameters.BackBufferWidth,
                _gfx.PresentationParameters.BackBufferHeight);
            
        }

        private void SetBlurOffsets(float dx, float dy)
        {
            _offsets[0] = Vector2.Zero;
            _gaussianKernel[0] = ComputeGaussian(0);

            float totalWeight = _gaussianKernel[0];
            
            for (var i = 0; i < KERNEL_SIZE / 2; i++)
            {
                float weight = ComputeGaussian(i + 1);
                float offset = i * 2 + 1.0f;

                totalWeight += weight;

                _gaussianKernel[i * 2 + 1] = weight;
                _gaussianKernel[i * 2 + 2] = weight;
                
                var delta = new Vector2(dx, dy) * offset;
                _offsets[i * 2 + 1] = delta;
                _offsets[i * 2 + 2] = -delta;
            }

            for (var i = 0; i < KERNEL_SIZE; i++)
            {
                _gaussianKernel[i] /= totalWeight;
            }

            _gaussian.Parameters["Kernel"].SetValue(_gaussianKernel);
            _gaussian.Parameters["Offsets"].SetValue(_offsets);
        }

        private float ComputeGaussian(float n)
        {
            float theta = _blurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
        
        private void SetBloomTexture(Texture2D texture)
        {
            _bloom.Parameters["BloomTexture"].SetValue(texture);
        }

        private void PerformBloom(RenderTarget2D frame, Rectangle rect)
        {
            var hWidth = (float) rect.Width;
            var hHeight = (float) rect.Height;

            _gfx.SetRenderTarget(_effectBuffer1);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _brightnessThreshold.CurrentTechnique.Passes[0].Apply();
            _batch.Draw(frame, rect, Color.White);
            _batch.End();

            _gfx.SetRenderTarget(_effectBuffer2);

            SetBlurOffsets(1.0f / hWidth, 0f);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _gaussian.CurrentTechnique.Passes[0].Apply();
            _batch.Draw(_effectBuffer1, rect, Color.White);
            _batch.End();

            _gfx.SetRenderTarget(_effectBuffer1);

            SetBlurOffsets(0f, 1f / hHeight);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _gaussian.CurrentTechnique.Passes[0].Apply();
            _batch.Draw(_effectBuffer2, rect, Color.White);
            _batch.End();

            _gfx.SetRenderTarget(_effectBuffer2);

            SetBloomTexture(_effectBuffer1);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _bloom.CurrentTechnique.Passes[0].Apply();
            _batch.Draw(frame, rect, Color.White);
            _batch.End();

            _gfx.SetRenderTarget(_intermediate);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _batch.Draw(_effectBuffer2, rect, Color.White);
            _batch.End();

        }

        private void NoEffect(RenderTarget2D renderTarget, Rectangle rect)
        {
            _gfx.SetRenderTarget(_intermediate);

            _batch.Begin();
            _batch.Draw(renderTarget, rect, Color.White);
            _batch.End();
        }

        private void SetShadowMaskParams()
        {
            _shadowmask.Parameters["TextureSize"].SetValue(_intermediate.Bounds.Size.ToVector2());
            _shadowmask.Parameters["OutputSize"].SetValue(_intermediate.Bounds.Size.ToVector2());
            _shadowmask.Parameters["HardPix"].SetValue(_hardPix);
            _shadowmask.Parameters["HardScan"].SetValue(_hardScan);
            _shadowmask.Parameters["BrightnessBoost"].SetValue(2f);
            _shadowmask.Parameters["MaskDark"].SetValue(0.8f);
            _shadowmask.Parameters["MaskLight"].SetValue(1.5f);
        }
        
        public void Process(RenderTarget2D renderTarget)
        {
            var rect = renderTarget.Bounds;

            if (EnableBloom)
            {
                PerformBloom(renderTarget, rect);
            }
            else
            {
                NoEffect(renderTarget, rect);
            }

            if (EnableShadowMask)
            {
                SetShadowMaskParams();
                
                // copy the intermediate RT to effect buffer 1
                // with the shadowmask effect applied
                _gfx.SetRenderTarget(_effectBuffer1);
                _batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                _shadowmask.CurrentTechnique.Passes[0].Apply();
                _batch.Draw(_intermediate, rect, Color.White);
                _batch.End();
                
                // copy effect buffer 1 back into the intermediate buffer
                _gfx.SetRenderTarget(_intermediate);
                _batch.Begin();
                _batch.Draw(_effectBuffer1, rect, Color.White);
                _batch.End();
            }
            
            
            
            _gfx.SetRenderTarget(null);

            _batch.Begin();
            _batch.Draw(_intermediate, rect, Color.White);
            _batch.End();
        }
    }
}