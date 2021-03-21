using RedTeam.IO;

namespace RedTeam.Commands
{
    public class ListDirectory : Command
    {
        public override string Name => "ls";

        protected override void Main(string[] args)
        {
            var dir = WorkingDirectory;
            if (args.Length > 0)
            {
                dir = ResolvePath(args[0]);
            }

            if (!FileSystem.DirectoryExists(dir))
            {
                Console.WriteLine("{0}: {1}: Directory not found.", Name, dir);
                return;
            }
            
            foreach (var subdir in FileSystem.GetDirectories(dir))
            {
                var name = PathUtils.GetFileName(subdir);

                Console.Write(name);
                Console.Write("    ");
            }
            
            foreach (var subdir in FileSystem.GetFiles(dir))
            {
                var name = PathUtils.GetFileName(subdir);

                Console.Write(name);
                Console.Write("    ");
            }

            Console.WriteLine();
        }
    }
}