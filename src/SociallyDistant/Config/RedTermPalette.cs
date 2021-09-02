using System;
using System.IO;
using System.Text.Json.Serialization;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui.Elements.Console;
using Thundershock.IO;

namespace SociallyDistant.Config
{
    public class RedTermPalette
    {
        [JsonPropertyName("id")]
        public string Id;
        
        [JsonPropertyName("name")]
        public string Name;
        
        [JsonPropertyName("description")]
        public string Description;
        
        [JsonPropertyName("author")]
        public string Author;

        [JsonPropertyName("background")]
        public RedTermBackground Background = new RedTermBackground();
        
        [JsonPropertyName("colors")]
        public RedTermColors Colors = new RedTermColors();
        
        [JsonPropertyName("completions")]
        public RedTermCompletions Completions = new RedTermCompletions();
        
        [JsonPropertyName("cursor")]
        public RedTermCursor Cursor = new RedTermCursor();
        
        [JsonPropertyName("redwm")]
        public WindowColors Redwm = new WindowColors();
        
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
                return null;
            }

            return null;
        }
        
        public ColorPalette ToColorPalette()
        {
            var palette = new ColorPalette();

            palette.BackgroundImage = LoadImage(Background.Image);
            
            Func<string, Color> html = Color.FromHtml;
            
            palette.SetColor(ConsoleColor.Black, html(Colors.Black) * Background.Opacity);
            palette.SetColor(ConsoleColor.DarkBlue, html(Colors.DarkBlue));
            palette.SetColor(ConsoleColor.DarkGreen, html(Colors.DarkGreen));
            palette.SetColor(ConsoleColor.DarkCyan, html(Colors.DarkCyan));
            palette.SetColor(ConsoleColor.DarkRed, html(Colors.DarkRed));
            palette.SetColor(ConsoleColor.DarkMagenta, html(Colors.DarkMagenta));
            palette.SetColor(ConsoleColor.DarkYellow, html(Colors.DarkYellow));
            palette.SetColor(ConsoleColor.Gray, html(Colors.Gray));
            palette.SetColor(ConsoleColor.DarkGray, html(Colors.DarkGray));
            palette.SetColor(ConsoleColor.Blue, html(Colors.Blue));
            palette.SetColor(ConsoleColor.Green, html(Colors.Green));
            palette.SetColor(ConsoleColor.Cyan, html(Colors.Cyan));
            palette.SetColor(ConsoleColor.Red, html(Colors.Red));
            palette.SetColor(ConsoleColor.Magenta, html(Colors.Magenta));
            palette.SetColor(ConsoleColor.Yellow, html(Colors.Yellow));
            palette.SetColor(ConsoleColor.White, html(Colors.White));
            
            // cursor
            palette.CursorColor = html(Cursor.Bg);
            palette.CursorForeground = html(Cursor.Fg);
            
            // completions
            palette.CompletionsBackground = html(Completions.Bg);
            palette.CompletionsText = html(Completions.Text);
            palette.CompletionsHighlight = html(Completions.Highlight);
            palette.CompletionsHighlightText = html(Completions.TextHighlight);

            palette.DefaultWindowColor = html(Redwm.Border);
            palette.PanicWindowColor = html(Redwm.PanicBorder);

            return palette;
        }
    }
}