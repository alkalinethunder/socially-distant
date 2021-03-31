using System;
using System.Linq;
using RedTeam.Net;

namespace RedTeam.Commands
{
    public class Ping : Command
    {
        private int _pingsLeft;
        private double _pingTime;
        private double _pingTimeTotal;
        private string _pingHost;
        private bool _pingTimeout;
        
        public override string Name => "ping";

        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <host>", Name);
                return;
            }

            var host = args.First();

            if (Network.DnsLookup(host, out uint address))
            {
                _pingHost = NetworkHelpers.ToIPv4String(address);
                _pingsLeft = 4;
                Console.WriteLine("Pinging {0} [{1}] with 32 bytes of data:", host, _pingHost);
                
                if (Network.GetPingTime(address, out double pingTime))
                {
                    _pingTimeTotal = pingTime / 1000;
                }
                else
                {
                    _pingTimeTotal = 5;
                    _pingTimeout = true;
                }
            }
            else
            {
                Console.WriteLine("Ping request could not find host {0}. Please check the name and try again.",
                    host);
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
                    if (_pingTimeout)
                    {
                        Console.WriteLine("Request timed out.");
                    }
                    else
                    {
                        Console.WriteLine("Reply from {0}: bytes=32 time={1}ms TTL=56", _pingHost,
                            Math.Round(_pingTimeTotal * 1000));
                    }

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