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
    public class ModalManager : SceneComponent
    {
        private GuiSystem _gui;

        private Texture2D _side;
        private Texture2D _corner;
        private Texture2D _bottom;
        private Texture2D _outer;
        private Texture2D _inner;
        private Texture2D _tSide;
        private Texture2D _title;
        private Texture2D _info;
        private SpriteFont _titleText;
        private SpriteFont _infoText;
        
        protected override void OnLoad()
        {
            base.OnLoad();
            
            _gui = Scene.GetComponent<GuiSystem>();

            ShowMessage("Attention!", "This game uses auto-save technology that doesn't work.");
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
    }
}