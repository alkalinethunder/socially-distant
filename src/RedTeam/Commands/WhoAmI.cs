namespace RedTeam.Commands
{
    public class WhoAmI : Command
    {
        public override string Name => "whoami";

        protected override void Main(string[] args)
        {
            Console.WriteLine(Context.UserName);
        }
    }
}