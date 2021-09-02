using System.Numerics;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class ChatBubble : ContentElement
    {
        private Stacker _outerStacker = new();
        private Panel _bubbleArea = new();
        private Picture _avatar = new();
        private Panel _spacer = new();
        private Stacker _bubbleStacker = new();
        private TextBlock _messageText = new();
        
        public Texture2D Avatar
        {
            get => _avatar.Image;
            set => _avatar.Image = value;
        }
        
        public ChatBubble(string message, bool isFromPlayer)
        {
            Children.Add(_outerStacker);

            if (isFromPlayer)
            {
                _outerStacker.Children.Add(_spacer);
                _outerStacker.Children.Add(_bubbleArea);
                _outerStacker.Children.Add(_avatar);

                _bubbleArea.HorizontalAlignment = HorizontalAlignment.Right;
                _messageText.TextAlign = TextAlign.Right;
                
                _bubbleArea.Padding = new Padding(0, 0, 15, 0);
            }
            else
            {
                _outerStacker.Children.Add(_avatar);
                _outerStacker.Children.Add(_bubbleArea);
                _outerStacker.Children.Add(_spacer);

                _bubbleArea.HorizontalAlignment = HorizontalAlignment.Left;
                _messageText.TextAlign = TextAlign.Left;

                _bubbleArea.Padding = new Padding(15, 0, 0, 0);
            }

            _spacer.BackColor = Color.Transparent;
            _bubbleArea.BackColor = _spacer.BackColor;

            _outerStacker.Direction = StackDirection.Horizontal;

            _spacer.Properties.SetValue(Stacker.FillProperty, new StackFill(0.6f));
            _bubbleArea.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _avatar.ImageMode = ImageMode.Rounded;
            _avatar.FixedWidth = 32;
            _avatar.FixedHeight = 32;

            _avatar.VerticalAlignment = VerticalAlignment.Top;

            _bubbleArea.Children.Add(_bubbleStacker);
            _bubbleStacker.Padding = 12f;

            _bubbleArea.VerticalAlignment = VerticalAlignment.Center;
            
            _messageText.Text = message;
            _bubbleStacker.Children.Add(_messageText);
        }

        protected override void OnPaint(GameTime gameTime, GuiRenderer renderer)
        {
            var color = Color.FromHtml("#1baaf7");

            var rounding = 5;

            var rect = _bubbleArea.ContentRectangle;

            var offset = new Vector2(rounding, rounding);

            var tl = rect.Location + offset;

            var br = (rect.Location + rect.Size) - offset;

            var bl = new Vector2(tl.X, br.Y);

            var tr = new Vector2(br.X, tl.Y);

            renderer.FillCircle(tl, rounding, color);
            renderer.FillCircle(tr, rounding, color);
            renderer.FillCircle(bl, rounding, color);
            renderer.FillCircle(br, rounding, color);

            var innerRect1 = new Rectangle(tl.X, rect.Y, rect.Width - (rounding * 2), rect.Height);
            var innerRect2 = new Rectangle(rect.X, tl.Y, rect.Width, rect.Height - (rounding * 2));
            
            renderer.FillRectangle(innerRect1, color);
            renderer.FillRectangle(innerRect2, color);
        }
    }
}