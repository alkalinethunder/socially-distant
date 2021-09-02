using System;
using System.Numerics;
using SociallyDistant.Gui.Elements;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Debugging;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Windowing
{
    public class WindowManager : ISystem
    {
        private Scene _scene;
        private static Type _globalDefaultTheme;
        
        private WindowTheme _theme;

        public Scene Scene => _scene;
        
        public WindowTheme Theme => _theme;

        public void LoadTheme<T>() where T : WindowTheme, new()
        {
            LoadThemeInternal(typeof(T));
        }

        public void Load()
        {
            
        }
        
        public void Init(Scene scene)
        {
            _scene = scene;
            
            if (_globalDefaultTheme != null)
            {
                LoadThemeInternal(_globalDefaultTheme);
            }
        }

        public void Unload()
        {
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Render(GameTime gameTime)
        {
        }
        
        [Cheat]
        public void Cheat_MakeInfoBox(string[] args)
        {
            ShowMessage("Cheat Message", string.Join(" ", args));
        }

        private ModalDialog MakeInfoBox(string title, string message)
        {
            var pane = CreateFloatingPane(title, WindowStyle.Dialog);
            
            var m = new ModalDialog(message);

            pane.Content.Add(m);
            
            return m;
        }

        public TextEntryDialog MakeTextPrompt(string title, string message)
        {
            var modal = MakeInfoBox(title, message);

            var entry = new TextEntryDialog(modal);

            return entry;
        }
        
        public void ShowMessage(string title, string message, Action action = null)
        {
            var m = MakeInfoBox(title, message);

            m.AddButton("OK", () =>
            {
                action?.Invoke();
                var p = m as Element;
                while (!(p is WindowFrame))
                {
                    p = p.Parent;
                }

                p.Parent.Children.Remove(p);
            });
        }

        public Pane CreatePane(string title)
        {
            var content = new Panel();
            var layout = new PaneLayout(this, title, content);
            var pane = new Pane(layout, content);

            return pane;
        }

        public WindowFrame CreateFloatingPane(string title, WindowStyle windowStyle)
        {
            var pane = new WindowFrame(this, windowStyle);
            pane.TitleText = title;
            
            _scene.Gui.AddToViewport(pane);

            pane.ViewportAlignment = new Vector2(0.5f, 0.5f);
            pane.ViewportAnchor = FreePanel.CanvasAnchor.Center;

            return pane;
        }

        private void LoadThemeInternal(Type type)
        {
            if (_theme != null)
            {
                _theme.Dispose();
                _theme = null;
            }

            var theme = (WindowTheme) Activator.CreateInstance(type, null);

            theme.Load(_scene.Graphics);
            
            _theme = theme;
        }

        public T OpenWindow<T>(WindowStyle style) where T : Window, new()
        {
            var winFrame = CreateFloatingPane("Window Title", style);
            var win = new T();

            win.Open(winFrame);

            return win;
        }
        
        public static void SetGlobalTheme<T>() where T : WindowTheme, new()
        {
            _globalDefaultTheme = typeof(T);
        }
    }
}