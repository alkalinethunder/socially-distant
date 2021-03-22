namespace RedTeam.Commands
{
    public class Cowsay : Command
    {
        public override string Name => "cowsay";
        protected override void Main(string[] args)
        {
            var text = "";
            while (Console.GetCharacter(out char ch))
            {
                text += ch;
            }

            Console.WriteLine(text);

            Console.WriteLine("cowsay cow is not yet implemented but pipes work.");

        }
    }
}