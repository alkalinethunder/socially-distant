using System.Linq;
using RedTeam.Game;
using RedTeam.Net;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam.Commands.Hacking
{
    public abstract class HackCommand : Command
    {
        private string _host;
        private string _address;
        private int _hops;
        private HackStartInfo _hack;
        
        protected override void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("{0}: usage: {0} <host> <port>");
                return;
            }

            var host = args.First();
            var portString = args[1];

            if (ushort.TryParse(portString, out ushort port))
            {
                if (Network.DnsLookup(host, out uint addr))
                {
                    var parsed = NetworkHelpers.ToIPv4String(addr);
                    if (Context.Network.TryGetHackable(addr, port, out int hops, out HackStartInfo startInfo))
                    {
                        _host = host;
                        _address = NetworkHelpers.ToIPv4String(addr);
                        _hops = hops;
                        _hack = startInfo;
                        StartHack();
                    }
                    else
                    {
                        Console.WriteLine("{0}: error: {1}:{2}: couldn't connect.", Name, host, port);
                        Complete();
                    }
                }
                else
                {
                    Console.WriteLine("{0}: {1}: Failed to resolve host.", Name, host);
                    Complete();
                }
            }
            else
            {
                Console.WriteLine("{0}: {1}:{2}: Not a valid port number. Port must be 0-65535.", Name, host,
                    portString);
                Complete();
            }
        }

        private void StartHack()
        {
            // So there are a few things we need to do before a hack command can run.
            //
            // First we need to start a trace if the network is, of course, being traced.
            //
            // Traces are effectively "timed hacks." That is, the player needs to GET IN, GET IT DONE,
            // and *GET THE FUCK OUT* before the timer runs out. If it runs out, then the Red Team Agency's
            // hacking tools will force a kernel panic in a last-stitched effort to keep the agency safe.
            // This kernel panic will fail the contract if any, and restart the player's computer.
            //
            // Trace time limits depend on the these free things. First is the difficulty. Next is the hops count.
            // Finally the time is determined by the amount of tunnelling layers between the player and the target
            // (that is, is the player hacking straight through their computer, or someone else's? How many layers of 
            // these tunnels are there?)
            //
            // Not all hackables have a trace.
            if (_hack.IsTraced)
            {
                // Trace time starts at 2 minutes.
                var traceTime = 120d;
                
                // The Difficulty level of the hackable will shave 30 seconds off that timer for each level.
                var timeShave = _hack.Difficulty switch
                {
                    Difficulty.Easy => 0,
                    Difficulty.Normal => 30,
                    Difficulty.Hard => 60,
                    Difficulty.Fucked => 90
                };

                traceTime -= timeShave;
                
                // 5 seconds are added for each hop.
                traceTime += (_hops - 1) * 5;
                
                // TODO: Tunneling levels bonus - 10s per level
                
                // Start the timer.
                EntryPoint.CurrentApp.GetComponent<RiskSystem>().SetTraceTimer(_hack, traceTime);
            }
            
            
        }
    }
}