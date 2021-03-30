using System.Collections.Generic;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class NetworkNode : WebNode
    {
        private Network _net;
        private IspNode _isp;
        private List<Device> _devs = new();

        public Network Network => _net;

        public override string Name => _net.DisplayName;
        public override uint Address => _net.PublicAddress;
        
        public NetworkNode(IspNode isp, Network net)
        {
            _isp = isp;
            _net = net;
        }
        
        public override WebNodeType Type => WebNodeType.Network;

        public override IEnumerable<WebNode> ConnectedNodes
        {
            get
            {
                yield return _isp;

                foreach (var dev in _devs)
                {
                    yield return new DeviceNode(this, dev);
                }
            }
        }

        public void AddDevice(Device device)
        {
            _devs.Add(device);
        }
    }
}