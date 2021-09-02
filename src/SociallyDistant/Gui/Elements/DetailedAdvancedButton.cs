using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class DetailedAdvancedButton : AdvancedButton
    {
        private Stacker _stacker = new();
        private Stacker _textStacker = new();
        private TextBlock _title = new();
        private TextBlock _description = new();
        private Picture _icon = new();

        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        public string Text
        {
            get => _description.Text;
            set => _description.Text = value;
        }

        public Texture2D Icon
        {
            get => _icon.Image;
            set => _icon.Image = value;
        }

        public DetailedAdvancedButton()
        {
            _title.WrapMode = TextWrapMode.WordWrap;
            _description.WrapMode = TextWrapMode.WordWrap;
            
            _textStacker.Children.Add(_title);
            _textStacker.Children.Add(_description);
            _textStacker.VerticalAlignment = VerticalAlignment.Center;
            
            _stacker.Children.Add(_icon);
            _stacker.Children.Add(_textStacker);
            _stacker.Direction = StackDirection.Horizontal;

            _icon.Margin = 2;
            _textStacker.Margin = 2;
            _stacker.Margin = 3;

            _textStacker.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _icon.VerticalAlignment = VerticalAlignment.Center;
            _icon.FixedWidth = 48;
            _icon.FixedHeight = 48;
            
            _title.ForeColor = Color.Cyan;
            _description.ForeColor = Color.FromHtml("#999999");
            
            Children.Add(_stacker);
        }
    }
}