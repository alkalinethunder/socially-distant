using System;
using System.Linq;

namespace SociallyDistant.Core.Commands
{
    public class DeleteCommand : Command
    {
        private bool _recursive;
        
        public override string Name => "rm";
        public override string Description => "Delete a file or directory.";
        
        private bool ApplyFlag(string flag)
        {
            switch (flag)
            {
                case "recursive":
                    _recursive = true;
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool ApplyOption(char option)
        {
            switch (option)
            {
                case 'r':
                    _recursive = true;
                    break;
                default:
                    return false;
            }

            return true;
        }
        
        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} [options] <path>", Name);
                return;
            }

            while (args.Any(x=>x.StartsWith("-")))
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (arg.StartsWith("--"))
                    {
                        var flag = arg.Substring("--".Length);
                        if (!ApplyFlag(flag))
                        {
                            Console.WriteLine("{0}: unexpected flag: {1}", Name, arg);
                            return;
                        }

                        for (var j = i + 1; j < args.Length; j++)
                        {
                            args[j - 1] = args[j];
                        }

                        Array.Resize(ref args, args.Length - 1);
                        break;
                    }
                    
                    if (arg.StartsWith("-"))
                    {
                        var flags = arg.Substring("-".Length);
                        foreach (var option in flags)
                        {
                            if (!ApplyOption(option))
                            {
                                Console.WriteLine("{0}: unexpected option: -{1}", Name, option);
                                return;
                            }
                        }

                        for (var j = i + 1; j < args.Length; j++)
                        {
                            args[j - 1] = args[j];
                        }

                        Array.Resize(ref args, args.Length - 1);
                        break;
                    }
                }
            }

            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} [options] <path>", Name);
                return;
            }

            var path = ResolvePath(args.First());

            FileSystem.Delete(path, _recursive);
        }
    }
}