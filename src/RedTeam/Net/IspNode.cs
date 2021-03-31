using System.Collections.Generic;
using System.Linq;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class IspNode : WebNode
    {
        private RegionNode _region;
        private InternetServiceProvider _isp;
        private List<NetworkNode> _nets = new List<NetworkNode>();

        public override string Name => _isp.Name;
        public override uint Address => _isp.NetworkAddress;
        
        public IspNode(RegionNode region, InternetServiceProvider isp)
        {
            _region = region;
            _isp = isp;
        }

        public InternetServiceProvider Isp => _isp;
        
        public override WebNodeType Type => WebNodeType.Isp;

        public override IEnumerable<WebNode> ConnectedNodes
        {
            get
            {
                yield return _region;

                foreach (var net in _nets)
                    yield return net;
            }
        }

        public void AddNetwork(Network net)
        {
            _nets.Add(new NetworkNode(this, net));
        }

        public NetworkNode GetNetwork(Network net)
        {
            return _nets.First(x => x.Network.Id == net.Id);
        }

        public WebNode NetworkLookup(uint address)
        {
            // Check the address to see if it's in our network.
            if ((address & _isp.SubnetMask) == (_isp.NetworkAddress & _isp.SubnetMask))
            {
                // If it is, then find the first network of ours whose address matches.
                return _nets.FirstOrDefault(x => x.Address == address);
            }
            
            // Send it to the region to do a lookup.
            return _region.NetworkLookup(address);
        }
    }
}