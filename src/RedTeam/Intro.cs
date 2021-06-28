using System.Numerics;
using Thundershock;
using Thundershock.Audio;
using Thundershock.Components;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Core.Rendering;
using Thundershock.Rendering;

namespace RedTeam
{
    public class Intro : Scene
    {
        #region Resources

        private Font _atFont;
        private Texture2D _atCircle;
        private string _at = "Alkaline Thunder";
        private Color _atColor = ThundershockPlatform.HtmlColor("#0376bd");

        #endregion

        #region State

        private int _state = 0;
        private float _atFade = 0;
        private double _atTime = 0;

        #endregion

        #region Components

        private TextComponent _atText = new();
        private Sprite _atLogoSprite = new();
        private Transform2D _atSpriteTransform = new();
        private Transform2D _atTextTransform = new();

        #endregion

        protected override void OnLoad()
        {
            _atFont = Font.FromResource(Game.Graphics, this.GetType().Assembly,
                "RedTeam.Resources.Fonts.AlkalineThunder.ttf");
            _atFont.Size = 38;

            _atCircle = Texture2D.FromResource(Game.Graphics, this.GetType().Assembly,
                "RedTeam.Resources.Brand.atcomputercircle.png");

            var atTextEntity = SpawnObject();
            var atSpriteEntity = SpawnObject();
            
            atTextEntity.AddComponent(_atTextTransform);
            atTextEntity.AddComponent(_atText);
            
            atSpriteEntity.AddComponent(_atSpriteTransform);
            atSpriteEntity.AddComponent(_atLogoSprite);

            _atLogoSprite.Texture = _atCircle;
            _atText.Font = _atFont;

            _atText.Text = _at;

            _atText.Font.Size = (int)(_atLogoSprite.Size.Y - (_atLogoSprite.Size.Y / 4));

            _atTextTransform.Position.X += (_atLogoSprite.Size.X / 2) + 3;
            
            PrimaryCamera.GetComponent<Transform>().Scale = Vector3.One * 312;

            var measure = _atFont.MeasureString(_at).X;
            _atSpriteTransform.Position.X -= (measure / 2) + 3;

            base.OnLoad();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            switch (_state)
            {
                case 0:
                    PrimaryCameraSettings.BackgroundColor = Color.Black;
                    
                    _atText.Color = Color.Transparent;
                    _atLogoSprite.Color = Color.Transparent;
                    
                    _atTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_atTime >= 1)
                    {
                        _state++;
                        _atTime = 0;
                    }
                    break;
                case 1:
                    _atText.Color = Color.Transparent;
                    _atLogoSprite.Color = Color.White * _atFade;

                    _atFade = MathHelper.Clamp(_atFade + (float) gameTime.ElapsedGameTime.TotalSeconds * 4, 0, 1);
                    if (_atFade >= 1)
                    {
                        _atFade = 0;
                        _state++;
                        _atLogoSprite.Color = Color.White;
                    }
                    
                    break;
                case 2:
                    _atTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_atTime >= 1)
                    {
                        _atTime = 0;
                        _state++;
                    }

                    break;
                case 3:
                    PrimaryCameraSettings.BackgroundColor = Color.Lerp(Color.White, _atColor, _atFade);
                    _atText.Color = Color.White * _atFade;
                    
                    _atFade = MathHelper.Clamp(_atFade + (float) gameTime.ElapsedGameTime.TotalSeconds * 2, 0, 1);
                    if (_atFade >= 1)
                    {
                        _atFade = 0;
                        _state++;
                        _atText.Color = Color.White;
                        PrimaryCameraSettings.BackgroundColor = _atColor;
                    }

                    break;
                case 4:
                    PrimaryCameraSettings.BloomIntensity += (float) gameTime.ElapsedGameTime.TotalSeconds;
                    PrimaryCameraSettings.BloomBlurAmount += (float) gameTime.ElapsedGameTime.TotalSeconds;

                    var scale = PrimaryCamera.GetComponent<Transform>().Scale;
                    scale.X -= (float) gameTime.ElapsedGameTime.TotalSeconds * 8;
                    scale.Y = scale.X;
                    PrimaryCamera.GetComponent<Transform>().Scale = scale;
                    break;
            }
            
            base.OnUpdate(gameTime);
        }
    }
}