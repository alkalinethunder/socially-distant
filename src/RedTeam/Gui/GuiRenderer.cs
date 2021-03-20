﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RedTeam.Gui
{
    public class GuiRenderer
    {
        private float _opacity;
        private Color _masterTint;
        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;
        private Texture2D _white;
        private Rectangle _clip;

        public GuiRenderer(GraphicsDevice device, Texture2D white, SpriteBatch batch, float opacity, Color tint,
            Rectangle clip)
        {
            _white = white;
            _spriteBatch = batch;
            _device = device;
            _opacity = opacity;
            _masterTint = tint;
            _clip = clip;
        }

        public void FillRectangle(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_white, rect, color * _opacity);
        }
        
        public void DrawRectangle(Rectangle rect, Color color, int thickness)
        {
            var half = Math.Min(rect.Width, rect.Height) / 2;
            
            if (thickness >= half)
            {
                FillRectangle(rect, color);
                return;
            }
            
            var left = new Rectangle(rect.Left, rect.Top, thickness, rect.Height);
            var right = new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height);
            var top = new Rectangle(left.Right, left.Top, rect.Width - (thickness * 2), thickness);
            var bottom = new Rectangle(top.Left, rect.Bottom - thickness, top.Width, top.Height);

            FillRectangle(left, color);
            FillRectangle(top, color);
            FillRectangle(right, color);
            FillRectangle(bottom, color);
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, Color color,
            TextAlign textAlign = TextAlign.Left, int dropShadow = 0)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            
            var tint = color * _opacity;

            if (dropShadow != 0)
            {
                DrawString(font, text, position + new Vector2(dropShadow, dropShadow), Color.Black, textAlign);
            }

            if (textAlign == TextAlign.Left)
            {
                _spriteBatch.DrawString(font, text, position, tint);
            }
            else
            {
                var lines = text.Split(Environment.NewLine);
                var width = font.MeasureString(text).X;

                foreach (var line in lines)
                {
                    var m = font.MeasureString(line);

                    var pos = position;

                    if (textAlign == TextAlign.Right)
                    {
                        pos.X += (width - m.X);
                    }
                    else if (textAlign == TextAlign.Center)
                    {
                        pos.X += (width - m.X) / 2;
                    }
                    
                    _spriteBatch.DrawString(font, line, pos, tint);
                    
                    position.Y += m.Y;
                }
            }
            
        }
    }
}