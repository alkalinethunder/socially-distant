using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Thundershock;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Styling;

namespace RedTeam.Gui.Styles
{
    public class HackerStyle : GuiStyle
    {
        private SpriteFont _paragraph;
        
        // Color palette...
        private Color _bg = ThundershockPlatform.HtmlColor("#222222");
        private Color _bgInset = Color.Black;
        private Color _cyan = ThundershockPlatform.HtmlColor("#00ffff");
        private Color _peace = ThundershockPlatform.HtmlColor("#1baaf7");
        private Color _danger = ThundershockPlatform.HtmlColor("#ff0000");
        private Color _dangerBright = ThundershockPlatform.HtmlColor("#f71b1b");
        private Color _warning = Color.Orange;
        private Color _text = ThundershockPlatform.HtmlColor("#eeeeee");
        private Color _textBright = Color.White;

        public override SpriteFont StringListFont => DefaultFont;
        public override int ProgressBarHeight => 5;

        public override SpriteFont DefaultFont => _paragraph;
        public override int CheckSize => 12;
        public override int TextCursorWidth => 1;
        public override Color DefaultForeground => _text;
        public override SpriteFont ButtonFont => _paragraph;

        protected override void OnLoad()
        {
            // Load fonts in.
            _paragraph = Gui.App.Content.Load<SpriteFont>("Fonts/Console/Regular");
            
            
            base.OnLoad();
        }

        public override Color GetButtonTextColor(IButtonElement button)
        {
            if (button.IsActive && button.IsPressed)
                return _bgInset;
            return _textBright;
        }

        public override void DrawSelectionBox(GuiRenderer renderer, Rectangle bounds, SelectionStyle selectionStyle)
        {
            var color = selectionStyle switch
            {
                SelectionStyle.TextHighlight => _peace,
                SelectionStyle.ItemHover => _warning * 0.5f,
                SelectionStyle.ItemActive => _warning,
                _ => throw new ArgumentOutOfRangeException(nameof(selectionStyle), selectionStyle, null)
            };

            renderer.FillRectangle(bounds, color);
        }

        public override void DrawCheckBox(GuiRenderer renderer, CheckBox checkBox, Rectangle bounds)
        {
            var borderColor = Color.Black; 
            var fillColor = Color.Black;

            if (checkBox.IsChecked)
            {
                fillColor = _peace;
            }
            else
            {
                fillColor = _bg;
            }

            if (checkBox.IsHovered)
            {
                borderColor = _peace;
                fillColor = fillColor.Lighten(0.5f);
            }
            else
            {
                borderColor = _text;
            }
            
            renderer.FillRectangle(bounds, _bgInset);

            renderer.DrawRectangle(bounds, borderColor, 1);

            bounds.X += 4;
            bounds.Y += 4;
            bounds.Width -= 8;
            bounds.Height -= 8;
            renderer.FillRectangle(bounds, fillColor);
        }

        public override void DrawProgressBar(GuiRenderer renderer, ProgressBar progressBar)
        {
            renderer.FillRectangle(progressBar.BoundingBox, _bgInset);
            renderer.DrawRectangle(progressBar.BoundingBox, _dangerBright, 1);

            var bounds = progressBar.BoundingBox;
            bounds.X += 2;
            bounds.Y += 2;
            bounds.Width -= 4;
            bounds.Height -= 4;

            bounds.Width = (int) (bounds.Width * progressBar.Value);

            renderer.FillRectangle(bounds, _danger);
        }

        public override void DrawTextCursor(GuiRenderer renderer, Color color, Vector2 position, int height)
        {
            renderer.FillRectangle(new Rectangle((int) position.X, (int) position.Y, TextCursorWidth, height), color);
        }

        public override void DrawButton(GuiRenderer renderer, IButtonElement button)
        {
            var color = Color.Black;

            if (button.IsActive)
                color = (button.ButtonActiveColor ?? StyleColor.Default).GetColor(_cyan);
            else
                color = (button.ButtonColor ?? StyleColor.Default).GetColor(_dangerBright);

            var isFilled = button.IsHovered;

            if (button.IsPressed)
                color = color.Darken(0.5f);

            if (isFilled)
                renderer.FillRectangle(button.BoundingBox, color);
            else
                renderer.DrawRectangle(button.BoundingBox, color, 2);
        }

        public override void DrawStringListBackground(GuiRenderer renderer, StringList stringList)
        {
            renderer.FillRectangle(stringList.BoundingBox, _bgInset);
        }

        public override void DrawListItem(GuiRenderer renderer, StringList stringList, Rectangle bounds, bool isActive, bool isHovered,
            string text)
        {
            if (isActive)
                DrawSelectionBox(renderer, bounds, SelectionStyle.ItemActive);
            else if (isHovered)
                DrawSelectionBox(renderer, bounds, SelectionStyle.ItemHover);

            renderer.DrawString(StringListFont, text, bounds.Location.ToVector2(), _textBright);
        }
    }
}