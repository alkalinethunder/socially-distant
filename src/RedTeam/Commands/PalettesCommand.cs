using System;
using System.Linq;
using RedTeam.Config;

namespace RedTeam.Commands
{
    public class PalettesCommand : Command
    {
        public override string Name => "palette";

        private string _help = @"usage:
    {0} list            - list available color palettes
    {0} set <palette>   - change the active redterm palette
    {0} reset           - reset to the default color palette
    {0} reload          - reload the current color palette
    {0} help            - show this screen
";

        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: error: Too few arguments.", Name);
                Console.WriteLine(_help, Name);
                return;
            }

            var config = RedTeamGame.Instance.GetComponent<ConfigurationManager>();
            var action = args.First();

            if (action == "help")
            {
                Console.WriteLine(_help, Name);
                return;
            }

            if (action == "list")
            {
                Console.WriteLine("Available redterm palettes:");
                Console.WriteLine();

                var palettes = config.GetPalettes();

                var longestName = palettes.OrderByDescending(x => x.name.Length).First().name.Length;

                foreach (var palette in palettes)
                {
                    Console.Write(" - {0}", palette.name);
                    for (var i = 0; i < palette.name.Length - longestName; i++)
                        Console.Write(" ");

                    Console.WriteLine(" - &w{0}", palette.description);
                    Console.WriteLine();
                    Console.WriteLine("Author: {0}", palette.author);
                    Console.WriteLine("{0} set {1}&W", Name, palette.id);
                    Console.WriteLine();
                }

                Console.WriteLine("Download more redterm palettes and create your own at");
                Console.WriteLine("#b&b&u{0}&0", RedTeamPlatform.Community);
                
                return;
            }

            if (action == "set")
            {
                var actionArgs = args.Skip(1).ToArray();
                if (!actionArgs.Any())
                {
                    Console.WriteLine("{0}: {1}: Too few arguments", Name, action);
                    Console.WriteLine(_help, Name);
                    return;
                }

                var palettes = config.GetPalettes();

                var paletteId = actionArgs.First();

                if (palettes.All(x => x.id != paletteId))
                {
                    Console.WriteLine("{0}: {1}: error: {2}: palette not found", Name, action, paletteId);
                    return;
                }

                config.ActiveConfig.RedTermPalette = paletteId;
                config.ApplyChanges();
                return;
            }
            
            Console.WriteLine("{0}: {1}: unrecognized command", Name, action);
            Console.WriteLine(_help, Name);
        }
    }
}