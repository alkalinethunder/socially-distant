using System.Collections.Generic;
using System.Linq;
using SociallyDistant.Core.SaveData;

namespace SociallyDistant.Core.Net
{
    public class NetworkNode : WebNode
    {
        private Network _net;
        private IspNode _isp;
        private List<DeviceNode> _devs = new();

        public Network Network => _net;

        public IspNode Isp => _isp;
        
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
                    yield return dev;
                }
            }
        }

        public void AddDevice(Device device)
        {
            _devs.Add(new DeviceNode(this, device));
        }

        public DeviceNode DeviceLookup(uint deviceAddress)
        {
            return _devs.FirstOrDefault(x => x.Device.LocalAddress == deviceAddress);
        }
        
        public DeviceNode GetDevice(Device dev)
        {
            return _devs.First(x => x.Device.Id == dev.Id);
        }
    }
}