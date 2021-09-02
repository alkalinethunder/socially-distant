using System.Numerics;
using Thundershock.Core;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class Throbber : ContentElement
    {
        private float _circleScale = 0;
        private Color _innerColor = Color.FromHtml("#1baaf7");
        private Color _outerColor = Color.Black;

        public float ThrobberSize { get; set; } = 16;
        
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return new(ThrobberSize, ThrobberSize);
        }
        
        protected override void OnPaint(GameTime gameTime, GuiRenderer renderer)
        {
            _circleScale += (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (_circleScale >= 1)
            {
                _circleScale = 0;
                var s = _innerColor;
                _innerColor = _outerColor;
                _outerColor = s;
            }

            var center = ContentRectangle.Center;
            var radius = ThrobberSize / 2;

            renderer.FillCircle(center, radius, _outerColor);
            renderer.FillCircle(center, radius * _circleScale, _innerColor * ((_circleScale / 2) + 0.5f));
        }
    }
}