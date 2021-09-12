namespace SociallyDistant.Shell.Commands
{
    public class PrintWorkingDirectory : Command
    {
        public override string Name => "pwd";
        public override string Description => "Print the full path to the current working directory.";
        
        protected override void Main(string[] args)
        {
            Console.WriteLine(WorkingDirectory);
        }
    }
}