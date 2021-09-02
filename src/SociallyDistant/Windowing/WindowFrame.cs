using System;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Windowing
{
    public class WindowFrame : Element
    {
        private WindowManager _wm;
        private bool _canDrag;

        private bool _dragging;

        private WindowStyle _windowStyle;
        private Panel _clientArea = new();
        private Panel _dragSurface = new();
        private Panel _nonClientArea = new();

        public string TitleText { get; set; } = "Window";
        
        public override bool CanPaint => true;
        
        public ElementCollection Content => _clientArea.Children;

        public WindowManager WindowManager => _wm;

        public WindowStyle WindowStyle => _windowStyle;
        
        public WindowFrame(WindowManager wm, WindowStyle windowStyle)
        {
            // Window style.
            _windowStyle = windowStyle;
            
            // Only Dialogs and Windows can be dragged.
            _canDrag = windowStyle == WindowStyle.Window || windowStyle == WindowStyle.Dialog;
            _wm = wm ?? throw new ArgumentNullException(nameof(wm));

            _nonClientArea.BackColor = Color.Transparent;
            _clientArea.BackColor = Color.Transparent;
            _dragSurface.BackColor = Color.Transparent;
            _dragSurface.VerticalAlignment = VerticalAlignment.Top;
            
            _nonClientArea.Children.Add(_dragSurface);
            _nonClientArea.Children.Add(_clientArea);
            Children.Add(_nonClientArea);
            
            _dragSurface.MouseDown += DragSurfaceOnMouseDown;
            _dragSurface.MouseUp += DragSurfaceOnMouseUp;
            
            IsInteractable = true;
            _dragSurface.IsInteractable = true;
        }
        
        private void OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            if (_dragging)
            {
                var delta = GuiSystem.ScreenToViewport(e.DeltaX, e.DeltaY);
                var pos = ViewportPosition;
                pos += delta;
                ViewportPosition = pos;
            }
        }

        private void DragSurfaceOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragging && _canDrag && e.Button == MouseButton.Primary)
            {
                GuiSystem.GlobalMouseMove -= OnMouseMove;
                _dragging = false;
            }
        }

        private void DragSurfaceOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_canDrag && e.Button == MouseButton.Primary)
            {
                GuiSystem.GlobalMouseMove += OnMouseMove;
                _dragging = true;
            }
        }
        
        protected override void OnPaint(GameTime gameTime, GuiRenderer renderer)
        {
            _clientArea.Padding = _wm.Theme.GetClientPadding(this);
            _dragSurface.FixedHeight = _clientArea.Padding.Top;

            _wm.Theme.PaintWindow(gameTime, renderer, this);
        }
    }
}