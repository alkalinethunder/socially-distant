using Thundershock.Core;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class StatusPanel : LayoutElement
    {
        private Stacker _master = new();
        private Panel _leftSide = new();
        private Panel _middle = new();
        private Panel _rightSide = new();
        private Picture _leftBg = new();
        private Picture _rightBg = new();
        private Picture _leftTransition = new();
        private Picture _rightTransition = new();
        private Picture _bg = new();
        private TextBlock _host = new();
        private TextBlock _fps = new();

        public Color Color
        {
            get => _leftBg.Tint;
            set => SetColors(value);
        }
        
        public string HostText
        {
            get => _host.Text;
            set => _host.Text = value;
        }

        public string FrameRate
        {
            get => _fps.Text;
            set => _fps.Text = value;
        }
        
        public StatusPanel()
        {
            _fps.ForeColor = Color.Black;
            _host.ForeColor = Color.Black;
            
            _leftSide.Children.Add(_leftBg);
            _rightSide.Children.Add(_rightBg);
            _middle.Children.Add(_bg);

            _leftSide.Children.Add(_host);
            _rightSide.Children.Add(_fps);

            _master.Children.Add(_leftSide);
            _master.Children.Add(_leftTransition);
            _master.Children.Add(_middle);
            _master.Children.Add(_rightTransition);
            _master.Children.Add(_rightSide);

            _master.FixedHeight = 20;

            _middle.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _master.Direction = StackDirection.Horizontal;
            Children.Add(_master);
        }

        private void SetColors(Color color)
        {
            _leftBg.Tint = color;
            _leftTransition.Tint = color;
            _rightBg.Tint = color;
            _rightTransition.Tint = color;
            _bg.Tint = color;
        }
    }
}