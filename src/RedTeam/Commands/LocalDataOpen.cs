namespace RedTeam.Commands
{
    public class LocalDataOpen : Command
    {
        public override string Name => "localdata";
        public override string Description => "Open the redwm client configuration in the host file manager.";

        protected override void Main(string[] args)
        {
            RedTeamPlatform.OpenFile(RedTeamPlatform.LocalDataPath);
        }
    }
}