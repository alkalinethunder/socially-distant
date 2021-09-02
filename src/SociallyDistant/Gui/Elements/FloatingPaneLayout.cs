using System.Numerics;
using SociallyDistant.Windowing;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class FloatingPaneLayout : LayoutElement, IPaneLayout
    {
        private bool _isDragging;
        private FreePanel.CanvasAnchor _last;
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

        public WindowManager WindowManager => _wm;
        
        public Color Color
        {
            get => _left.Tint;
            set => SetColors(value);
        }
        
        public FloatingPaneLayout(WindowManager manager, string titleText, Panel content)
        {
            _wm = manager;
            var client = content;

            client.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
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
            _contentArea.Children.Add(client);
            _contentArea.Children.Add(_right);

            _bottomArea.Children.Add(_bLeft);
            _bottomArea.Children.Add(_bottom);
            _bottomArea.Children.Add(_bRight);

            _master.Children.Add(_titleArea);
            _master.Children.Add(_contentArea);
            _master.Children.Add(_bottomArea);
            
            _titleText.VerticalAlignment = VerticalAlignment.Center;
            _titleText.TextAlign = TextAlign.Center;
            _titleText.Text = titleText;
            
            _titleArea.Direction = StackDirection.Horizontal;
            _contentArea.Direction = StackDirection.Horizontal;
            _bottomArea.Direction = StackDirection.Horizontal;

            _titleArea.IsInteractable = true;
            
            _titleArea.MouseDown+= HandleDragStart;
            _titleArea.MouseUp += HandleDragEnd;
            _titleArea.MouseMove += HandleDrag;
            
            Children.Add(_master);
            _master.IsInteractable = true;
        }

        private void HandleDrag(object sender, MouseMoveEventArgs e)
        {
            if (_isDragging)
            {
                var grandpa = Parent.Parent.ContentRectangle;
                var deltaViewport = GuiSystem.ScreenToViewport(new Vector2(e.DeltaX, e.DeltaY));
                var offset = deltaViewport / grandpa.Size;

                _last.Left += offset.X;
                _last.Top += offset.Y;

                Parent.Properties.SetValue(FreePanel.AnchorProperty, _last);
            }
        }

        private void HandleDragEnd(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        private void HandleDragStart(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                var parent = Parent;
                if (parent.Properties.ContainsKey(FreePanel.AnchorProperty))
                {
                    _last = parent.Properties.GetValue<FreePanel.CanvasAnchor>(FreePanel.AnchorProperty);
                }
                else
                {
                    _last = FreePanel.CanvasAnchor.Center;
                }

                _isDragging = true;
            }
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