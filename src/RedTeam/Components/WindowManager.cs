using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Gui.Elements;
using Thundershock;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace RedTeam.Components
{
    public class WindowManager : SceneComponent
    {
        private GuiSystem _gui;
        
        protected override void OnLoad()
        {
            base.OnLoad();
            
            _gui = Scene.GetComponent<GuiSystem>();
        }

        private ModalDialog MakeInfoBox(string title, string message)
        {
            var m = new ModalDialog(this, title, message);
            m.HorizontalAlignment = HorizontalAlignment.Center;
            m.VerticalAlignment = VerticalAlignment.Center;
            _gui.AddToViewport(m);
            return m;
        }

        public void ShowMessage(string title, string message, Action action = null)
        {
            var m = MakeInfoBox(title, message);

            m.AddButton("OK", () =>
            {
                action?.Invoke();
                m.Parent.Children.Remove(m);
            });
        }

        public Pane CreatePane(string title)
        {
            var content = new Panel();
            var layout = new PaneLayout(this, title, content);
            var pane = new Pane(layout, content);

            return pane;
        }
    }
}