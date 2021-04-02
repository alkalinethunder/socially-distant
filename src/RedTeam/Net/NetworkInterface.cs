using System.Collections.Generic;
using System.Linq;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class NetworkInterface
    {
        private NetworkSimulation _simulation;
        private DeviceNode _devNode;
        
        
        public NetworkInterface(NetworkSimulation simulation, DeviceNode devNode)
        {
            _simulation = simulation;
            _devNode = devNode;
        }

        public uint LocalAddress
            => (NetworkAddress & SubnetMask) | (_devNode.Address & ~SubnetMask);

        public uint SubnetMask
            => _devNode.Network.SubnetMask;

        public uint PublicAddress
            => _devNode.Network.PublicAddress;

        public uint NetworkAddress
            => _devNode.Network.SubnetAddress;

        public bool DnsLookup(string host, out uint addr)
            => _simulation.TryMapHostToAddress(host, out addr);

        public IEnumerable<PortScanResult> PerformPortScan(uint address, out int hops)
        {
            var node = MapAddressToNode(address, out hops);
            if (node is DeviceNode device)
            {
                // local area network! this is ideal.
                return _simulation.GetHackables(device.Device).Select(x => new PortScanResult(x, true));
            }
            else if (node is NetworkNode network)
            {
                // Generate port maps if that hasn't happened yet.
                _simulation.GeneratePortMappings(network.Network);
                
                // Public network. These need to be hacked into first.
                return _simulation.GetMappedHackables(network.Network).Select(x => new PortScanResult(x, false));
            }
            else
            {
                return null;
            }
        }

        public bool TryGetHackable(uint address, ushort port, out int hops, out HackStartInfo startInfo)
        {
            var result = false;
            hops = 0;
            startInfo = null;

            var node = MapAddressToNode(address, out hops);

            if (node is DeviceNode device)
            {
                var hackables = _simulation.GetHackables(device.Device);

                var matched = hackables.FirstOrDefault(x => x.Port == port);

                if (matched != null)
                {
                    startInfo = _simulation.StartHack(matched);
                    result = true;
                }
            }
            else if (node is NetworkNode network)
            {
                var mappedHackables = _simulation.GetMappedHackables(network.Network);

                var matched = mappedHackables.FirstOrDefault(x => x.Port == port);

                if (matched != null)
                {
                    if (!matched.HackableFlags.IsFirewalled)
                    {
                        startInfo = _simulation.StartHack(matched);
                        result = true;
                    }
                }
            }
            
            return result;
        }
        
        public WebNode MapAddressToNode(uint address, out int hops)
        {
            // we've gone nowhere!
            hops = 0;
            
            // Loop-back address (127.0.0.1) maps to ourself.
            if (address == NetworkHelpers.LoopbackAddress)
                return this._devNode;

            // We're about to leave this device and go to our LAN, that's one hop.
            hops++;
            
            // Is the specified address inside of our network?
            if ((address & this.SubnetMask) == this.NetworkAddress)
            {
                // Map to a device node on our network.
                return _devNode.FindOtherLocalDevice(address);
            }
            
            // Address isn't in our LAN, attempt a tier 3 lookup.
            else
            {
                return _devNode.NetworkLookup(address, ref hops);
            }
            
            

            return null;
        }

        public int GetHopsCount(WebNode destNode)
        {
            return _simulation.CalculateHops(this._devNode, destNode);
        }
    }
}