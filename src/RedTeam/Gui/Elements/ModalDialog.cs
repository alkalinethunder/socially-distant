using System;
using RedTeam.Components;
using Thundershock.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Thundershock.Gui;

namespace RedTeam.Gui.Elements
{
    public class ModalDialog : Element
    {
        private Panel _masterPanel = new();
        private Panel _clientPanel = new();
        private Picture _left = new Picture();
        private Picture _right = new Picture();
        private Picture _bLeft = new Picture();
        private Picture _bRight = new Picture();
        private Picture _bottom = new Picture();
        private Picture _toLeft = new Picture();
        private Picture _tiLeft = new Picture();
        private Picture _tsLeft = new Picture();
        private Picture _titleImage = new Picture();
        private Picture _tsRight = new Picture();
        private Picture _tiRight = new Picture();
        private Picture _toRight = new Picture();
        private TextBlock _titleText = new TextBlock();
        private Panel _titlePanel = new Panel();
        private Picture _infoIcon = new Picture();
        private Stacker _contentMasterStacker = new Stacker();
        private Stacker _infoStacker = new Stacker();
        private TextBlock _messageText = new TextBlock();
        private Stacker _masterStacker = new Stacker();
        private Stacker _titleStacker = new Stacker();
        private Stacker _contentStacker = new Stacker();
        private Stacker _bottomStacker = new Stacker();
        private SpriteFont _infoText;
        private SpriteFont _titleFont;
        private Texture2D _infoImage;
        private Texture2D _title;
        private Texture2D _outerImage;
        private Texture2D _innerImage;
        private Texture2D _sideImage;
        private Texture2D _tSideImage;
        private Texture2D _bottomImage;
        private Texture2D _cornerImage;
        private Stacker _buttonStacker = new();
        
        public ModalDialog(WindowManager manager, string title, string message)
        {
            _infoImage = manager.App.Content.Load<Texture2D>("Textures/InfoBox");
            _outerImage = manager.App.Content.Load<Texture2D>("WinDecorations/TopCornerOuter");
            _innerImage = manager.App.Content.Load<Texture2D>("WinDecorations/TopCornerInner");
            _tSideImage = manager.App.Content.Load<Texture2D>("WinDecorations/TitleSide");
            _title = manager.App.Content.Load<Texture2D>("WinDecorations/Title");
            
            _sideImage = manager.App.Content.Load<Texture2D>("WinDecorations/Side");
            _bottomImage = manager.App.Content.Load<Texture2D>("WinDecorations/Bottom");
            _cornerImage = manager.App.Content.Load<Texture2D>("WinDecorations/BottomCorner");

            _titleFont = manager.App.Content.Load<SpriteFont>("Fonts/Console/Bold");
            _infoText = manager.App.Content.Load<SpriteFont>("Fonts/Console/Regular");

            _buttonStacker.HorizontalAlignment = HorizontalAlignment.Center;
            _clientPanel.BackColor = Color.Black;
            _messageText.Font = _infoText;
            _messageText.Text = message;
            _messageText.Color = Color.Cyan;
            _messageText.VerticalAlignment = VerticalAlignment.Center;
            _messageText.Properties.SetValue<StackFill>(Stacker.FillProperty, StackFill.Fill);

            _infoIcon.FixedWidth = 48;
            _infoIcon.FixedHeight = 48;
            _infoIcon.VerticalAlignment = VerticalAlignment.Center;
            
            _infoStacker.Direction = StackDirection.Horizontal;

            _contentMasterStacker.Children.Add(_infoStacker);
            _contentMasterStacker.Children.Add(_buttonStacker);
            _infoStacker.Children.Add(_infoIcon);
            _infoStacker.Children.Add(_messageText);
            
            _infoIcon.Image = _infoImage;
            
            _titleText.Text = title;
            _titleText.Font = _titleFont;
            
            _toRight.Image = _outerImage;
            _toLeft.Image = _outerImage;
            _tiRight.Image = _innerImage;
            _tiLeft.Image = _innerImage;
            _tsRight.Image = _tSideImage;
            _tsLeft.Image = _tSideImage;
            _titleImage.Image = _title;

            _tsRight.SpriteEffects = SpriteEffects.FlipHorizontally;
            _toRight.SpriteEffects = SpriteEffects.FlipHorizontally;
            _tiRight.SpriteEffects = SpriteEffects.FlipHorizontally;

            _left.Image = _sideImage;
            _right.Image = _sideImage;
            _bLeft.Image = _cornerImage;
            _bRight.Image = _cornerImage;
            _bottom.Image = _bottomImage;

            _bRight.SpriteEffects = SpriteEffects.FlipHorizontally;
            
            _titlePanel.Children.Add(_titleImage);
            _titlePanel.Children.Add(_titleText);

            _titleText.TextAlign = TextAlign.Center;
            _titleText.VerticalAlignment = VerticalAlignment.Center;
            
            _titleStacker.Children.Add(_toLeft);
            _titleStacker.Children.Add(_tiLeft);
            _titleStacker.Children.Add(_tsLeft);
            _titleStacker.Children.Add(_titlePanel);
            _titleStacker.Children.Add(_tsRight);
            _titleStacker.Children.Add(_tiRight);
            _titleStacker.Children.Add(_toRight);

            _titleStacker.Direction = StackDirection.Horizontal;
            _contentStacker.Direction = StackDirection.Horizontal;
            _bottomStacker.Direction = StackDirection.Horizontal;

            _contentStacker.Children.Add(_left);
            _contentStacker.Children.Add(_clientPanel);
            _contentStacker.Children.Add(_right);

            _bottomStacker.Children.Add(_bLeft);
            _bottomStacker.Children.Add(_bottom);
            _bottomStacker.Children.Add(_bRight);
            
            _masterStacker.Children.Add(_titleStacker);
            _masterStacker.Children.Add(_contentStacker);
            _masterStacker.Children.Add(_bottomStacker);

            _masterPanel.Children.Add(_masterStacker);
            
            // Fills.
            _clientPanel.Properties.SetValue<StackFill>(Stacker.FillProperty, 1f);
            _tiLeft.Properties.SetValue<StackFill>(Stacker.FillProperty, 1f);
            _tiRight.Properties.SetValue<StackFill>(Stacker.FillProperty, 1f);
            _bottom.Properties.SetValue<StackFill>(Stacker.FillProperty, 1f);

            _clientPanel.Children.Add(_contentMasterStacker);

            _tiLeft.Tile = true;
            _tiRight.Tile = true;
            
            // Thundershock magic that guarantees the images seamlessly
            // line up.
            _tiLeft.WidthUnitRounding = _tiLeft.Image.Width;
            _tiRight.WidthUnitRounding = _tiRight.Image.Width;

            Children.Add(_masterPanel);

            _masterPanel.MaximumWidth = 900;

            _messageText.WrapMode = TextWrapMode.WordWrap;
        }

        public void AddButton(string text, Action action)
        {
            var btn = new Button();
            var txt = new TextBlock();

            txt.Text = text;
            txt.Font = _infoText;
            txt.TextAlign = TextAlign.Center;
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.Color = Color.White;
            
            btn.Children.Add(txt);

            btn.MouseUp += (_, _) => action?.Invoke();
            
            _buttonStacker.Children.Add(btn);
        }
    }
}