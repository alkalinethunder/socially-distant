using System;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Thundershock;
using Thundershock.IO;

namespace RedTeam.Config
{
    public class RedTermPalette
    {
        public string id;
        public string name;
        public string description;
        public string author;

        public RedTermBackground background = new RedTermBackground();
        
        public RedTermColors colors = new RedTermColors();
        public RedTermCompletions completions = new RedTermCompletions();
        public RedTermCursor cursor = new RedTermCursor();
        public WindowColors redwm = new WindowColors();
        
        private Texture2D LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            
            var pathParts = PathUtils.Split(path);
            if (pathParts[0] == "USERDATA:")
                pathParts[0] = ThundershockPlatform.LocalDataPath;

            var realPath = Path.Combine(pathParts);

            if (File.Exists(realPath))
            {
                try
                {
                    return Texture2D.FromFile(EntryPoint.CurrentApp.GraphicsDevice, realPath);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
        
        public ColorPalette ToColorPalette()
        {
            var palette = new ColorPalette();

            palette.BackgroundImage = LoadImage(background.image);
            
            Func<string, Microsoft.Xna.Framework.Color> html = ThundershockPlatform.HtmlColor;
            
            palette.SetColor(ConsoleColor.Black, html(colors.black) * this.background.opacity);
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

            palette.DefaultWindowColor = html(redwm.border);
            palette.PanicWindowColor = html(redwm.panicBorder);

            return palette;
        }
    }
}