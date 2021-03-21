namespace RedTeam.Commands
{
    public class PrintWorkingDirectory : Command
    {
        public override string Name => "pwd";
        protected override void Main(string[] args)
        {
            Console.WriteLine(WorkingDirectory);
        }
    }
}