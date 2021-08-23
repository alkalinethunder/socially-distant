using System;
using System.Numerics;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;

namespace SociallyDistant.Windowing
{
    public class WhiteCarbonTheme : WindowTheme
    {
        private GraphicsProcessor _gpu;
        private int _borderPad = 3;

        private const int _titleHeight = 28;

        private Font _titleFont;

        private Texture2D _winBG;
        
        private Color _amber = Color.FromHtml("#ffb000");
        private Color _peace = Color.FromHtml("#0054E3");
        
        protected override void OnLoad(GraphicsProcessor gpu)
        {
            _gpu = gpu;
            
            _titleFont = Font.FromResource(_gpu, GetType().Assembly, "SociallyDistant.Resources.Fonts.Console.Bold.ttf");
            _titleFont.Size = 20;

            _winBG = LoadResource("win_bg.png");
            
            base.OnLoad(gpu);
        }

        public override Padding GetClientPadding(WindowFrame win)
        {
            return win.WindowStyle switch
            {
                WindowStyle.Window => new Padding(2, _titleHeight, 2, 2),
                WindowStyle.Dialog => new Padding(2, _titleHeight, 2, 2),
                WindowStyle.Tile => new Padding(1, _titleHeight, 1, 1),
                WindowStyle.Banner => new Padding(2, 2, 2, 2),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void PaintWindow(GameTime gameTime, GuiRenderer renderer, WindowFrame win)
        {
            var client = win.BoundingBox;
            var padding = GetClientPadding(win);

            switch (win.WindowStyle)
            {
                case WindowStyle.Window:
                    PaintWindowFrame(gameTime, renderer, win, client, padding);
                    break;
                case WindowStyle.Dialog:
                    PaintDialogFrame(gameTime, renderer, win, client, padding);
                    break;
                case WindowStyle.Tile:
                    PaintTileFrame(gameTime, renderer, win, client, padding);
                    break;
                case WindowStyle.Banner:
                    PaintBannerFrame(gameTime, renderer, win, client, padding);
                    break;
            }
        }

        private void PaintWindowFrame(GameTime gameTime, GuiRenderer renderer, WindowFrame win, Rectangle bounds,
            Padding padding)
        {
        }

        private void PaintDialogFrame(GameTime gameTime, GuiRenderer renderer, WindowFrame win, Rectangle bounds,
            Padding padding)
        {
            var color = _amber;
            var inner = new Rectangle(bounds.Left + 1, bounds.Top + 1, bounds.Width - 2, bounds.Height - 2);
            
            // Border paint.
            renderer.FillRectangle(bounds, color);

            // Border paint if active.
            if (win.HasAnyFocus)
            {
                renderer.DrawRectangle(inner, Color.Black, 2);
            }
            
            // Client background.
            var client = new Rectangle(bounds.Left + padding.Left, bounds.Top + padding.Top,
                bounds.Width - padding.Width, bounds.Height - padding.Height);
            renderer.FillRectangle(client, Color.Black);
            
            // Title paint.
            var measure = _titleFont.MeasureString(win.TitleText);
            var titlePos = new Vector2(
                bounds.Left + ((bounds.Width - measure.X) / 2),
                bounds.Top + ((padding.Top - measure.Y) / 2)
            );
            renderer.DrawString(_titleFont, win.TitleText, titlePos, Color.Black);


        }

        private void PaintTileFrame(GameTime gameTime, GuiRenderer renderer, WindowFrame win, Rectangle bounds,
            Padding padding)
        {
            var u = 8f / _winBG.Width;
            var v = padding.Top / _winBG.Height;
            var uvLeft = new Rectangle(0, 0, u, v);
            var uvRight = new Rectangle(1 - u, 0, u, v);
            var uvTop = new Rectangle(uvLeft.Right, uvLeft.Top, uvRight.Left - uvLeft.Width, uvLeft.Bottom);
            var uvBottom = new Rectangle(0, uvTop.Bottom, 1, 1 - uvTop.Bottom);

            // Border paint.
            renderer.FillRectangle(new Rectangle(bounds.Left, bounds.Top, 8, padding.Top), _winBG, _peace, uvLeft);
            renderer.FillRectangle(new Rectangle(bounds.Right - 8, bounds.Top, 8, padding.Top), _winBG, _peace, uvRight);
            renderer.FillRectangle(new Rectangle(bounds.Left + 8, bounds.Top, bounds.Width - 16, padding.Top), _winBG,
                _peace, uvTop);
            renderer.FillRectangle(
                new Rectangle(bounds.Left, bounds.Top + padding.Top, bounds.Width, bounds.Height - padding.Top), _winBG,
                _peace, uvBottom);
            
            // Client paint.
            renderer.FillRectangle(
                new Rectangle(bounds.Left + padding.Left, bounds.Top + padding.Top, bounds.Width - padding.Width,
                    bounds.Height - padding.Height), Color.Black);
            
            // Title paint.
            var titlePos = new Vector2(bounds.Left + 8, bounds.Top + ((padding.Top - _titleFont.LineHeight) / 2));
            renderer.DrawString(_titleFont, win.TitleText, titlePos, Color.White, 2, Color.Gray * 0.7f);
        }

        private void PaintBannerFrame(GameTime gameTime, GuiRenderer renderer, WindowFrame win, Rectangle bounds,
            Padding padding)
        {

        }

        private Texture2D LoadResource(string resource)
        {
            var ass = GetType().Assembly;
            var fullname = "SociallyDistant.Resources.ThemeAssets.WhiteCarbon." + resource;
            return Texture2D.FromResource(_gpu, ass, fullname);
        }
    }
}