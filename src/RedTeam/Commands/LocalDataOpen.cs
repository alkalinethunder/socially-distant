namespace RedTeam.Commands
{
    public class LocalDataOpen : Command
    {
        public override string Name => "localdata";

        protected override void Main(string[] args)
        {
            RedTeamPlatform.OpenFile(RedTeamPlatform.LocalDataPath);
        }
    }
}