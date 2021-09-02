using System;
using System.Collections.Generic;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant.Commands
{
    public class Lolcat : Command
    {
        private const int SegmentWidth = 8;
        private const int Colors = 16;

        public override string Name => "lolcat";
        public override string Description => "Mmmmmmmm....tasty rainbows....";
        
        private bool ParseColorCode(string text, out int index)
        {
            var result = false;
            index = -1;

            if (text.Contains(ConsoleControl.ForegroundColorCode))
            {
                var i = text.IndexOf(ConsoleControl.ForegroundColorCode);
                var ch = i + 1;
                if (ch < text.Length)
                {
                    var chCode = text[ch];
                    var isColor = char.ToLower(chCode) switch
                    {
                        '0' => true,
                        '9' => true,
                        '8' => true,
                        '7' => true,
                        '6' => true,
                        '5' => true,
                        '4' => true,
                        '3' => true,
                        '2' => true,
                        '1' => true,
                        'a' => true,
                        'b' => true,
                        'c' => true,
                        'd' => true,
                        'e' => true,
                        'f' => true,
                        _ => false
                    };

                    if (isColor)
                    {
                        index = i;
                        result = true;
                    }
                }
            }
            
            return result;
        }
        
        private string RemoveForeColorCodes(string line)
        {
            while (ParseColorCode(line, out int index))
            {
                var bef = line.Substring(0, index);
                var aft = line.Substring(index + 2);
                line = bef + aft;
            }

            return line;
        }
        
        protected override void Main(string[] args)
        {
            // DISCLAIMER:
            //
            // Because of how redterm deals with console text wrapping, there is no way for lolcat to actually know
            // when text will end up on a newline without it seeing a \n character. As such, the rainbow effect may look
            // undesirable if text wrapping kicks in.
            //
            // Deal with it.
            //
            // This is a fucking game.
            // - Michael
            
            // First read in the text from the console.
            var lines = new List<string>();
            while (Console.GetLine(out string line))
            {
                // Make sure to get rid of foreground color codes
                lines.Add(RemoveForeColorCodes(line));
            }
            
            // rng
            var rng = new Random();
            
            // pick a random color
            var startColor = rng.Next(Colors - 1) + 1;

            // The character index of the start of the rainbow.
            var rbStart = 0;

            foreach (var line in lines)
            {
                // Current color
                var color = startColor;
                
                // line start
                var lineStart = rbStart;
                
                // go through the line
                while (lineStart <= line.Length)
                {
                    // get the segment length
                    var segLen = SegmentWidth;
                    
                    // Is there text yet?
                    if (lineStart + segLen > 0)
                    {
                        // get the end of the text
                        var end = Math.Min(lineStart + segLen, line.Length);
                        
                        // get the start
                        var start = Math.Max(0, lineStart);
                        
                        // and the count
                        var count = end - start;
                        
                        // and the text
                        var text = line.Substring(start, count);
                        
                        // write the color
                        Console.Write("{0}{1}", ConsoleControl.ForegroundColorCode, color.ToString("x"));
                        
                        // and write the text
                        Console.Write(text);
                    }
                    
                    // increase the color
                    color++;
                    if (color >= Colors)
                        color = 1;

                    lineStart += segLen;
                }

                // decrease the rainbow start.
                rbStart--;
                
                // write a newline
                Console.WriteLine();
            }
        }
    }
}