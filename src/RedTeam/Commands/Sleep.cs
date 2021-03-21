using System.Linq;

namespace RedTeam.Commands
{
    public class Sleep : Command
    {
        private float _timeLeft;
        
        public override string Name => "sleep";

        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <milliseconds>", Name);
                return;
            }

            var time = args.First();

            if (!float.TryParse(time, out _timeLeft))
            {
                Console.WriteLine("{0}: {1}: numeric value expected.", Name, time);
            }

            _timeLeft /= 1000f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timeLeft -= deltaTime;
            if (_timeLeft <= 0)
                Complete();
        }
    }
}