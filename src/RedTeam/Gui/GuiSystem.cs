using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Gui.Elements;

namespace RedTeam.Gui
{
    public sealed class GuiSystem : SceneComponent
    {
        private RootElement _rootElement;
        private bool _debugShowBounds = true;
        private SpriteFont _debugFont;

        protected override void OnLoad()
        {
            base.OnLoad();
            _rootElement = new RootElement();

            _debugFont = Game.Content.Load<SpriteFont>("Fonts/DebugSmall");
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
            
            PerformLayout();

            _rootElement.Update(gameTime);
        }

        private float ComputeElementOpacity(Element element)
        {
            var opacity = element.Opacity;
            var parent = element.Parent;
            while (parent != null)
            {
                opacity = opacity * parent.Opacity;
                parent = parent.Parent;
            }

            return opacity;
        }

        private void PerformLayout()
        {
            var screenRectangle = new Rectangle(0, 0, Game.ScreenWidth, Game.ScreenHeight);

            var rootLayout = _rootElement.LayoutManager;

            rootLayout.SetBounds(screenRectangle);
        }
        
        private Color ComputeElementTint(Element element)
        {
            var color = element.Enabled ? Color.White : Color.Gray;
            var parent = element.Parent;
            while (parent != null)
            {
                var pColor = parent.Enabled ? Color.White : Color.Gray;

                var r = (float) pColor.R / 255f;
                var g = (float) pColor.G / 255f;
                var b = (float) pColor.B / 255f;

                var br = (byte) (color.R * r);
                var bg = (byte) (color.G * g);
                var bb = (byte) (color.B * b);

                color = new Color(br, bg, bb);
                
                parent = parent.Parent;
            }

            return color;
        }

        protected override void OnDraw(GameTime gameTime, SpriteBatch batch)
        {
            base.OnDraw(gameTime, batch);

            foreach (var element in _rootElement.CollapseElements())
            {
                var opacity = ComputeElementOpacity(element);
                var masterTint = ComputeElementTint(element);
                var clip = Rectangle.Empty;
                
                var renderer = new GuiRenderer(Game.GraphicsDevice, Game.White, batch, opacity, masterTint, clip);

                batch.Begin();
                element.Paint(gameTime, renderer);
                batch.End();

                if (_debugShowBounds)
                {
                    var debugRenderer = new GuiRenderer(Game.GraphicsDevice, Game.White, batch, 1, Color.White,
                        Rectangle.Empty);

                    batch.Begin();

                    debugRenderer.DrawRectangle(element.BoundingBox, Color.White, 1);

                    var text = $"{element.Name}{Environment.NewLine}BoundingBox={element.BoundingBox}";
                    var measure = _debugFont.MeasureString(text);
                    var pos = new Vector2((element.BoundingBox.Left + ((element.BoundingBox.Width - measure.X) / 2)),
                        element.BoundingBox.Top + ((element.BoundingBox.Height - measure.Y) / 2));

                    debugRenderer.DrawString(_debugFont, text, pos, Color.White, TextAlign.Center, 2);
                    
                    
                    batch.End();
                }
            }
        }
    }
}