using System;
using Thundershock;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Windowing
{
    public abstract class Window : ContentElement
    {
        private WindowFrame WindowFrame
        {
            get
            {
                var p = Parent;
                while (p != null)
                {
                    if (p is WindowFrame win)
                        return win;
                    p = p.Parent;
                }

                return null;
            }
        }

        public string Title
        {
            get => WindowFrame.TitleText;
            set => WindowFrame.TitleText = value;
        }

        protected Scene Scene
        {
            get
            {
                var win = WindowFrame;
                var wm = win.WindowManager;
                return wm.Scene;
            }
        }
        
        public void Close()
        {
            if (OnClosed())
                WindowFrame?.RemoveFromParent();
        }

        public void Open(WindowFrame windowFrame)
        {
            windowFrame.Content.Add(this);
            OnOpened();
        }
        
        protected virtual void OnOpened() {}

        protected virtual bool OnClosed()
        {
            WindowClosed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public event EventHandler WindowClosed;
    }
}