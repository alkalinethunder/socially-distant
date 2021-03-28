using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Thundershock;

namespace RedTeam.Config
{
    public class RedTermPalette
    {
        public string id;
        public string name;
        public string description;
        public string author;

        public RedTermColors colors = new RedTermColors();
        public RedTermCompletions completions = new RedTermCompletions();
        public RedTermCursor cursor = new RedTermCursor();
        
        public ColorPalette ToColorPalette()
        {
            var palette = new ColorPalette();

            Func<string, Microsoft.Xna.Framework.Color> html = ThundershockPlatform.HtmlColor;
            
            palette.SetColor(ConsoleColor.Black, html(colors.black));
            palette.SetColor(ConsoleColor.DarkBlue, html(colors.darkBlue));
            palette.SetColor(ConsoleColor.DarkGreen, html(colors.darkGreen));
            palette.SetColor(ConsoleColor.DarkCyan, html(colors.darkCyan));
            palette.SetColor(ConsoleColor.DarkRed, html(colors.darkRed));
            palette.SetColor(ConsoleColor.DarkMagenta, html(colors.darkMagenta));
            palette.SetColor(ConsoleColor.DarkYellow, html(colors.darkYellow));
            palette.SetColor(ConsoleColor.Gray, html(colors.gray));
            palette.SetColor(ConsoleColor.DarkGray, html(colors.darkGray));
            palette.SetColor(ConsoleColor.Blue, html(colors.blue));
            palette.SetColor(ConsoleColor.Green, html(colors.green));
            palette.SetColor(ConsoleColor.Cyan, html(colors.cyan));
            palette.SetColor(ConsoleColor.Red, html(colors.red));
            palette.SetColor(ConsoleColor.Magenta, html(colors.magenta));
            palette.SetColor(ConsoleColor.Yellow, html(colors.yellow));
            palette.SetColor(ConsoleColor.White, html(colors.white));
            
            // cursor
            palette.CursorColor = html(cursor.bg);
            palette.CursorForeground = html(cursor.fg);
            
            // completions
            palette.CompletionsBackground = html(completions.bg);
            palette.CompletionsText = html(completions.text);
            palette.CompletionsHighlight = html(completions.highlight);
            palette.CompletionsHighlightText = html(completions.textHighlight);
            
            return palette;
        }
    }
}