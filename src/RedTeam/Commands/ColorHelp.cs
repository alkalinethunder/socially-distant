using System;
using System.Linq;

namespace RedTeam.Commands
{
    public class ColorHelp : Command
    {
        public override string Name => "colors";

        protected override void Main(string[] args)
        {
            Console.WriteLine("redterm formatting guide");
            Console.WriteLine();
            Console.WriteLine("precede text with ## followed by hex digit to set foreground color");
            Console.WriteLine("or precede text with $$ followed by hex digit to set background color");
            Console.WriteLine();

            var colors = new int[16];
            for (var i = 0; i < colors.Length; i++)
                colors[i] = i;

            var names = colors.Select(x => ((ConsoleColor) x).ToString().ToLower()).ToArray();

            var longestName = names.OrderByDescending(x => x.Length).First().Length;
            var longestExample = names.Select(x => x + " text").OrderByDescending(x => x.Length).First().Length;

            Console.Write("name  ");
            for (var i = 0; i < longestName - 4; i++)
                Console.Write(" ");
            Console.Write("bg code  ");
            Console.Write("fg code  ");
            Console.Write("bg     ");
            Console.WriteLine("fg");

            for (var i = 0; i < colors.Length; i++)
            {
                var name = names[i];
                var ex = name + " text";
                var code = i.ToString("x");
                
                Console.Write(name);
                Console.Write("  ");
                for (var j = 0; j < longestName - name.Length; j++)
                    Console.Write(" ");
                Console.Write("$${0}       ", code);
                Console.Write("##{0}       ", code);
                Console.Write("${0}     &0  #{0}{1} text&0", code, name);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("use double-sequences to escape literal characters (e.g: ### for literal ##)");
            Console.WriteLine("a formatting code followed by an unrecognized value will be rendered as literal text");
            Console.WriteLine();
            Console.WriteLine("text can be &bbold&B (&&b), &iitalic&I (&&i) or &b&ibold-italic&B&I (&&b followed by &&i). ");
            Console.WriteLine(
                "use upper-case versions (&&B, &&I) to disable individual font styles or &&1 to reset to default font.");
            Console.WriteLine();
            Console.WriteLine("text can also be &uunderlined&U (&&u = underline, &&U = disable)");
            Console.WriteLine();
            Console.WriteLine(
                "#e$9&bAll three formatting codes can be mixed to make text look like this.&0 - &&0 will reset the entire terminal");
            Console.WriteLine();
            Console.WriteLine("text can also &2#fblink with the &&2 code&0");
        }
    }
}