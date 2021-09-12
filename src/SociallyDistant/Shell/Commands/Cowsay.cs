using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SociallyDistant.Shell.Commands
{
    public class Cowsay : Command
    {
        public override string Name => "cowsay";

        public override string Description =>
            "Have the almighty cow use its voice of reason. The cowsay cow holds authority over all of man-kind.";
        protected override void Main(string[] args)
        {
            var cow = GetCow();
            var text = "";
            while (Console.GetCharacter(out char ch))
            {
                text += ch;
            }

            var lines = BreakText(text, 45);

            var bubble = MakeBubble(lines);

            Console.Write(bubble);
            Console.WriteLine(cow);
        }

        private string MakeBubble(string[] lines)
        {
            var top = '_';
            var bottom = '-';
            var topLeft = '/';
            var topRight = '\\';
            var side = '|';
            var sideLeft = '<';
            var sideRight = '>';

            if (!lines.Any())
            {
                return $"{sideLeft}{sideRight}";
            }
            
            var maxLength = lines.OrderByDescending(x => x.Length).First().Length;

            var sb = new StringBuilder();
            sb.Append(" ");
            for (var i = 0; i < maxLength + 2; i++)
                sb.Append(top);
            sb.AppendLine(" ");

            if (lines.Length == 1)
            {
                var line = lines.First();
                sb.Append(sideLeft);
                sb.Append(' ');
                sb.Append(line);
                sb.Append(' ');                
                sb.Append(sideRight);
                sb.AppendLine();
            }
            else
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    var l = side;
                    var r = side;

                    if (i == 0)
                    {
                        l = topLeft;
                        r = topRight;
                    }
                    else if (i == lines.Length - 1)
                    {
                        l = topRight;
                        r = topLeft;
                    }

                    sb.Append(l);
                    sb.Append(' ');
                    sb.Append(line);
                    for (var j = 0; j < maxLength - line.Length; j++)
                        sb.Append(' ');
                    sb.Append(' ');
                    sb.Append(r);
                    sb.AppendLine();
                }
            }
            
            sb.Append(" ");
            for (var i = 0; i < maxLength + 2; i++)
                sb.Append(bottom);
            sb.AppendLine(" ");

            return sb.ToString();
        }
        
        private string[] BreakText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            var words = new List<string>();
            var word = string.Empty;
            for (var i = 0; i <= text.Length; i++)
            {
                if (i == text.Length)
                {
                    if (!string.IsNullOrEmpty(word))
                        words.Add(word);
                }
                else
                {
                    var ch = text[i];
                    word += ch;
                    if (char.IsWhiteSpace(ch))
                    {
                        words.Add(word);
                        word = string.Empty;
                    }
                }
            }

            var lines = new List<string>();
            var line = string.Empty;

            for (var i = 0; i < words.Count; i++)
            {
                var w = words[i];

                if (line.Length + w.Length > maxLength)
                {
                    lines.Add(line.TrimEnd());
                    line = string.Empty;
                }

                while (w.Length > maxLength)
                {
                    var substr = w.Substring(0, maxLength);
                    lines.Add(substr.TrimEnd());
                    w = w.Substring(substr.Length);
                }

                line += w;

                if (line.EndsWith('\n'))
                {
                    lines.Add(line.TrimEnd());
                    line = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(line))
            {
                lines.Add(line.TrimEnd());
                line = string.Empty;
            }
            
            return lines.ToArray();
        }
        
        private string GetCow()
        {
            var resource = GetType().Assembly.GetManifestResourceStream("SociallyDistant.Resources.Cowsay.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }
    }
}