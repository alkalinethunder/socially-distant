using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Thundershock.Gui.Elements;

namespace RedTeam.Gui.Elements
{
    public class StatusPanel : Element
    {
        private Stacker _master = new();
        private Panel _leftSide = new();
        private Panel _middle = new();
        private Panel _rightSide = new();
        private Picture _leftBG = new();
        private Picture _rightBG = new();
        private Picture _leftTransition = new();
        private Picture _rightTransition = new();
        private Picture _bg = new();
        private RedTeamHackerScene _desktop;
        private TextBlock _host = new();
        private TextBlock _fps = new();

        public Color Color
        {
            get => _leftBG.Tint;
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
        
        public StatusPanel(RedTeamHackerScene desktop)
        {
            _desktop = desktop;

            var font = _desktop.App.Content.Load<SpriteFont>("Fonts/Console/Regular");
            var side = _desktop.App.Content.Load<Texture2D>("Textures/Panel/InfoBG");
            var transition = _desktop.App.Content.Load<Texture2D>("Textures/Panel/InfoTransition");
            var bg = _desktop.App.Content.Load<Texture2D>("Textures/Panel/BG");

            _fps.Font = font;
            _host.Font = font;
            
            _fps.Color = Color.Black;
            _host.Color = Color.Black;
            
            _bg.Image = bg;
            _leftBG.Image = side;
            _rightBG.Image = side;
            _rightTransition.Image = transition;
            _leftTransition.Image = transition;

            _rightTransition.SpriteEffects = SpriteEffects.FlipHorizontally;
            _rightBG.SpriteEffects = SpriteEffects.FlipHorizontally;
            
            _leftSide.Children.Add(_leftBG);
            _rightSide.Children.Add(_rightBG);
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
            _leftBG.Tint = color;
            _leftTransition.Tint = color;
            _rightBG.Tint = color;
            _rightTransition.Tint = color;
            _bg.Tint = color;
        }
    }
}