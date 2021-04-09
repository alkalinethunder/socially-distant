using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Components;
using Thundershock;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace RedTeam.Gui.Elements
{
    public class PaneLayout : Element
    {
        private Panel _client;
        private WindowManager _wm;
        private Stacker _master = new();
        private Stacker _titleArea = new();
        private Stacker _contentArea = new();
        private Stacker _bottomArea = new();
        private Picture _left = new();
        private Picture _right = new();
        private Picture _bottom = new();
        private Picture _bLeft = new();
        private Picture _bRight = new();
        private Picture _tLeft = new();
        private Panel _titleBg = new();
        private Picture _titlePicture = new();
        private TextBlock _titleText = new();
        private Picture _titleSide = new();
        private Picture _stripes = new();
        private Picture _tRight = new();

        public Color Color
        {
            get => _left.Tint;
            set => SetColors(value);
        }
        
        public PaneLayout(WindowManager manager, string titleText, Panel content)
        {
            var title = manager.App.Content.Load<Texture2D>("WinDecorations/Title");
            var outerBright = manager.App.Content.Load<Texture2D>("WinDecorations/OuterCornerBright");
            var side = manager.App.Content.Load<Texture2D>("WinDecorations/Side");
            var bottom = manager.App.Content.Load<Texture2D>("WinDecorations/Bottom");
            var corner = manager.App.Content.Load<Texture2D>("WinDecorations/BottomCorner");
            var outer = manager.App.Content.Load<Texture2D>("WinDecorations/TopCornerOuter");
            var stripes = manager.App.Content.Load<Texture2D>("WinDecorations/TopCornerInner");
            var tSide = manager.App.Content.Load<Texture2D>("WinDecorations/TitleSide");
            
            
            _wm = manager;
            _client = content;

            _client.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _contentArea.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _bottom.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _stripes.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _titleBg.Children.Add(_titlePicture);
            _titleBg.Children.Add(_titleText);
            
            _titleArea.Children.Add(_tLeft);
            _titleArea.Children.Add(_titleBg);
            _titleArea.Children.Add(_titleSide);
            _titleArea.Children.Add(_stripes);
            _titleArea.Children.Add(_tRight);
            
            _contentArea.Children.Add(_left);
            _contentArea.Children.Add(_client);
            _contentArea.Children.Add(_right);

            _bottomArea.Children.Add(_bLeft);
            _bottomArea.Children.Add(_bottom);
            _bottomArea.Children.Add(_bRight);

            _master.Children.Add(_titleArea);
            _master.Children.Add(_contentArea);
            _master.Children.Add(_bottomArea);

            _left.Image = side;
            _right.Image = side;
            _bottom.Image = bottom;
            _bLeft.Image = corner;
            _bRight.Image = corner;
            _tLeft.Image = outerBright;
            _titlePicture.Image = title;
            _tRight.Image = outer;
            _stripes.Image = stripes;
            _titleSide.Image = tSide;
            
            _titleText.VerticalAlignment = VerticalAlignment.Center;
            _titleText.TextAlign = TextAlign.Center;
            _titleText.Text = titleText;
            _titleText.Font = manager.App.Content.Load<SpriteFont>("Fonts/Console/Bold");
            
            _bRight.SpriteEffects = SpriteEffects.FlipHorizontally;
            _tRight.SpriteEffects = SpriteEffects.FlipHorizontally;
            _titleSide.SpriteEffects = SpriteEffects.FlipHorizontally;
            _stripes.SpriteEffects = SpriteEffects.FlipHorizontally;

            _stripes.WidthUnitRounding = _stripes.Image.Width;
            
            _titleArea.Direction = StackDirection.Horizontal;
            _contentArea.Direction = StackDirection.Horizontal;
            _bottomArea.Direction = StackDirection.Horizontal;

            _stripes.Tile = true;
            
            Children.Add(_master);
        }

        private void SetColors(Color color)
        {
            _left.Tint = color;
            _right.Tint = color;
            _titleSide.Tint = color;
            _titlePicture.Tint = color;
            _bottom.Tint = color;
            _stripes.Tint = color;
            _tLeft.Tint = color;
            _tRight.Tint = color;
            _bLeft.Tint = color;
            _bRight.Tint = color;
        }
    }
}