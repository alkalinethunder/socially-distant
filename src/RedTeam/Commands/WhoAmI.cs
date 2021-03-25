namespace RedTeam.Commands
{
    public class WhoAmI : Command
    {
        public override string Name => "whoami";
        public override string Description => "Print the currently logged in user's name.";
        
        protected override void Main(string[] args)
        {
            Console.WriteLine(Context.UserName);
        }
    }
}