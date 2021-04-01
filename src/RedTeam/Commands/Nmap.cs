using System;
using System.Linq;
using RedTeam.Net;

namespace RedTeam.Commands
{
    public class Nmap : Command
    {
        private string _parsedAddress;
        private PortScanResult[] _scanResults;
        private bool _timedOut;
        private double _timeoutIn;
        private double _timePerScan;
        private double _nextScanIn;
        private int _nextScan;
        private int _longestPort;
        private int _longestStatus;
        private int _longestId;
        
        public override string Name => "nmap";
        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <host>", Name);
                return;
            }

            var host = args.First();
            
            // dns resolution
            if (Network.DnsLookup(host, out uint address))
            {
                _parsedAddress = NetworkHelpers.ToIPv4String(address);
                Console.WriteLine("Nmap scan report for {0} ({1})",
                    host, _parsedAddress);

                var scanResults = Context.Network.PerformPortScan(address, out int hops);

                if (scanResults != null)
                {
                    _scanResults = scanResults.OrderBy(x => x.Port).ToArray();
                    _timePerScan = 0.050 * hops;
                    _longestPort = _scanResults.Select(x => x.Port.ToString() + "/tcp").OrderByDescending(x => x.Length)
                        .Select(x => x.Length).FirstOrDefault();
                    _longestId = _scanResults.Select(x => x.Id).OrderByDescending(x => x.Length).Select(x => x.Length)
                        .FirstOrDefault();
                    _longestStatus = _scanResults.Select(x => x.Status).OrderByDescending(x => x.Length)
                        .Select(x => x.Length).FirstOrDefault();

                    _longestStatus = Math.Max(_longestStatus, 5);
                }
                else
                {
                    _timedOut = true;
                    _timeoutIn = 5;
                }

            }
            else
            {
                Console.WriteLine("this is the text that appears when a dns lookup fails in the port scanner.");
                Complete();
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_timedOut)
            {
                _timeoutIn -= deltaTime;
                if (_timeoutIn <= 0)
                {
                    Console.WriteLine("this is the text that appears after the portscanner has a time-out.");
                    Complete();
                    return;
                }
            }
            else
            {
                if (_nextScan >= _scanResults.Length)
                {
                    Complete();
                    return;
                }

                _nextScanIn += deltaTime;
                if (_nextScanIn >= _timePerScan)
                {
                    if (_nextScan == 0)
                    {
                        Console.WriteLine("Host is up");
                        Console.WriteLine("Not shown {0} closed ports", 1000 - _scanResults.Length);
                        Console.Write("PORT");
                        for (var i = 0; i < _longestPort - 4; i++)
                            Console.Write(" ");
                        Console.Write(" STATE");
                        for (var i = 0; i < _longestStatus - 5; i++)
                            Console.Write(" ");
                        Console.WriteLine(" SERVICE");
                    }

                    var port = _scanResults[_nextScan].Port.ToString();
                    var state = _scanResults[_nextScan].Status;
                    var service = _scanResults[_nextScan].Id;

                    Console.Write(port + "/tcp");
                    for (var i = 0; i < _longestPort - (port + "/tcp").Length; i++)
                    {
                        Console.Write(" ");
                    }

                    Console.Write(" " + state);
                    for (var i = 0; i < _longestStatus - state.Length; i++)
                    {
                        Console.Write(" ");
                    }

                    Console.WriteLine(" {0}", service);

                    _nextScanIn = 0;
                    _nextScan++;
                }
            }
        }
    }
}