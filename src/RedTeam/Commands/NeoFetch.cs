namespace RedTeam.Commands
{
    public class NeoFetch : Command
    {
        public override string Name => "neofetch";
        protected override void Main(string[] args)
        {
            for (var i = 0; i < 16; i++)
            {
                var hex = i.ToString("x");
                Console.WriteLine(ConsoleControl.BackgroundColorCode + hex + "    " +
                                  ConsoleControl.BackgroundColorCode + "0");
            }
        }
    }
}