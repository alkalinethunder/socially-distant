using System;
using System.Linq;

namespace RedTeam.Commands
{
    public class Ping : Command
    {
        private int _pingsLeft;
        private double _pingTime;
        private double _pingTimeTotal;
        private string _pingHost;
        
        public override string Name => "ping";

        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <host>", Name);
                return;
            }

            var host = args.First();

            if (Network.GetPingTime(host, out double pingTime, out string resolved))
            {
                _pingHost = resolved;
                _pingTimeTotal = pingTime / 1000;
                _pingsLeft = 4;
                Console.WriteLine("Pinging {0} [{1}] with 32 bytes of data:", host, resolved);
            }
            else
            {
                Console.WriteLine("Ping request could not find host {0}. Please check the name and try again.", host);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_pingsLeft <= 0)
            {
                Complete();
            }
            else
            {
                if (_pingTime >= _pingTimeTotal + 0.5)
                {
                    Console.WriteLine("Reply from {0}: bytes=32 time={1}ms TTL=56", _pingHost,
                        Math.Round(_pingTimeTotal * 1000));
                    _pingTime = 0;
                    _pingsLeft--;
                }
                else
                {
                    _pingTime += deltaTime;
                }
            }
        }
    }
}