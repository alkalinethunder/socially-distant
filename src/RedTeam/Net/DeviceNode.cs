using System.Collections.Generic;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class DeviceNode : WebNode
    {
        private Device _dev;
        private NetworkNode _net;

        public override string Name => _dev.HostName;
        public override uint Address => _dev.LocalAddress;
        
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
    }
}