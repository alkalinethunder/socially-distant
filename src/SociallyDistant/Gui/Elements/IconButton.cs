using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class IconButton : AdvancedButton
    {
        private Stacker _stacker = new();
        private Picture _icon = new();
        private TextBlock _text = new();
        
        public string Text
        {
            get => _text.Text;
            set => _text.Text = value;
        }
        
        public Texture2D Image
        {
            get => _icon.Image;
            set => _icon.Image = value;
        }

        public StackDirection Direction
        {
            get => _stacker.Direction;
            set => _stacker.Direction = value;
        }

        public float IconSize
        {
            get => _icon.FixedWidth;
            set
            {
                _icon.FixedWidth = value;
                _icon.FixedHeight = value;
            }
        }

        public IconButton()
        {
            _stacker.Children.Add(_icon);
            _stacker.Children.Add(_text);

            _icon.Padding = 4;
            _text.TextAlign = TextAlign.Center;

            Children.Add(_stacker);

            ButtonColor = Color.Transparent;
            _text.ForeColor = Color.White;

            Direction = StackDirection.Vertical;
            IconSize = 48;
        }

        protected override bool OnMouseEnter(MouseMoveEventArgs e)
        {
            _icon.Tint = Color.White * 0.8f;
            _text.Opacity = 1;
            return base.OnMouseEnter(e);
        }

        protected override bool OnMouseLeave(MouseMoveEventArgs e)
        {
            _icon.Tint = Color.White;
            _text.Opacity = 0.75f;

            return base.OnMouseLeave(e);
        }
    }
}