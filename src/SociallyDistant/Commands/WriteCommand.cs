using System.Linq;

namespace SociallyDistant.Commands
{
    public class WriteCommand : Command
    {
        public override string Name => "write";

        public override string Description =>
            "Write text over an existing file or create a new file with the given text.";

        protected override void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("{0}: usage: {0} <file> <contents>", Name);
                return;
            }

            var path = args[0];
            var text = string.Join(" ", args.Skip(1).ToArray());

            var resolved = ResolvePath(path);

            FileSystem.WriteAllText(resolved, text);
        }
    }
}