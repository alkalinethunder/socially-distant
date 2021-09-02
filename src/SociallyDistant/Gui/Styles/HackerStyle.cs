using System;
using System.Numerics;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Styling;

namespace SociallyDistant.Gui.Styles
{
    public class HackerStyle : GuiStyle
    {
        private Font _paragraph;

        private Font _h1;
        private Font _h2;
        private Font _h3;
        private Font _code;
        private Font _stringListFont;
        private Font _buttonFont;

        // Color palette...
        private Color _bg = Color.FromHtml("#222222");
        private Color _bgInset = Color.Black;
        private Color _amber = Color.FromHtml("#ffbf00");
        private Color _peace = Color.FromHtml("#1baaf7");
        private Color _danger = Color.FromHtml("#ff0000");
        private Color _dangerBright = Color.FromHtml("#f71b1b");
        private Color _warning = Color.Orange;
        private Color _text = Color.White * 0.9f;
        private Color _textBright = Color.White;

        public override int ProgressBarHeight => 5;

        public override Font DefaultFont => _paragraph;
        public override int CheckSize => 12;
        public override int TextCursorWidth => 1;
        public override Color DefaultForeground => _text;

        protected override void OnLoad()
        {
            _paragraph = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.AndikaNewBasic-Regular.ttf");
            _h1 = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.Console.Bold.ttf");
            _h2 = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.Console.Bold.ttf");
            _h3 = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.Console.Bold.ttf");
            _buttonFont = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.AndikaNewBasic-Regular.ttf");
            _code = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.Console.Regular.ttf");
            _stringListFont = Font.FromResource(Gui.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.Console.Regular.ttf");

            _paragraph.Size = 26;
            _code.Size = _paragraph.Size;
            _h3.Size = _paragraph.Size + 2;
            _h2.Size = _h3.Size + 4;
            _h1.Size = _h2.Size + 8;

            _buttonFont.Size = _paragraph.Size;
            _stringListFont.Size = _buttonFont.Size;
            
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
                _ => Color.Transparent
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
                color = (button.ButtonActiveColor ?? StyleColor.Default).GetColor(_amber);
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

            renderer.DrawString(_stringListFont, text, bounds.Location, _textBright);
        }

        public override Font GetFont(Element element)
        {
            if (element is StringList)
            {
                return _stringListFont;
            }
            else if (element is Button)
            {
                return _buttonFont;
            }
            else
            {
                var style = element.Properties.GetValue<FontStyle>();

                return style switch
                {
                    FontStyle.Paragraph => _paragraph,
                    FontStyle.Heading1 => _h1,
                    FontStyle.Heading2 => _h2,
                    FontStyle.Heading3 => _h3,
                    FontStyle.Code => _code,
                    _ => _paragraph
                };
            }
        }

        public override void PaintElementBackground(Element element, GameTime gameTime, GuiRenderer renderer)
        {
            var backColor = element.BackColor ?? StyleColor.Default;
            renderer.FillRectangle(element.BoundingBox, backColor.GetColor(_bg));
        }

        public override TextRenderBuffer PaintMenuItemText(Element element, GameTime gameTime, GuiRenderer renderer, string text, Font font,
            Vector2 textPos, SelectionStyle selectionStyle)
        {
            return font.Draw(text, textPos, _text, renderer.Layer);
        }
    }
}