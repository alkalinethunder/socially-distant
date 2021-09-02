using System;
using System.Linq;

namespace SociallyDistant.Core.Commands
{
    public class MakeDirectory : Command
    {
        public override string Name => "mkdir";
        public override string Description => "Make a directory.";
        
        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <path>", Name);
                return;
            }

            var path = ResolvePath(args.First());

            try
            {
                FileSystem.CreateDirectory(path);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("{0}: {1}: {2}", Name, path, ex.Message);
            }
        }
    }
}