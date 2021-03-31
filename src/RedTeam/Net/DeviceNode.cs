using System.Collections.Generic;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class DeviceNode : WebNode
    {
        private Device _dev;
        private NetworkNode _net;

        public Network Network => _net.Network;
        
        public override string Name => _dev.HostName;
        public override uint Address => _net.Network.SubnetAddress | _dev.LocalAddress;

        public Device Device => _dev;
        
        public DeviceNode(NetworkNode net, Device dev)
        {
            _net = net;
            _dev = dev;
        }

        public override WebNodeType Type => WebNodeType.Device;

        public override IEnumerable<WebNode> ConnectedNodes
        {
            get
            {
                yield return _net;
            }
        }

        public WebNode FindOtherLocalDevice(uint address)
        {
            return _net.DeviceLookup(address & ~Network.SubnetMask);
        }

        public WebNode NetworkLookup(uint address, ref int hops)
        {
            // if the address matches our network, return our network.
            if (address == _net.Address)
                return _net;
            
            // otherwise do a lookup with our ISP.
            hops++;
            return _net.Isp.NetworkLookup(address, ref hops);
        }
    }
}