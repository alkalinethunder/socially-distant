using System.Collections.Generic;
using System.Linq;
using SociallyDistant.SaveData;

namespace SociallyDistant.Net
{
    public class RegionNode : WebNode
    {
        private MasterWebNode _master;
        private RegionNetwork _region;
        private List<IspNode> _isps = new();
        
        public RegionNetwork Region => _region;

        public override string Name => _region.Name;
        public override uint Address => _region.RegionAddress;
        
        public RegionNode(MasterWebNode master, RegionNetwork region)
        {
            _master = master;
            _region = region;
        }

        public override WebNodeType Type => WebNodeType.Region;

        public override IEnumerable<WebNode> ConnectedNodes
        {
            get
            {
                yield return _master;

                foreach (var isp in _isps)
                    yield return isp;
            }
        }

        public IspNode GetIsp(InternetServiceProvider isp)
        {
            return _isps.First(x => x.Isp.Id == isp.Id);
        }

        public void AddIsp(InternetServiceProvider isp)
        {
            _isps.Add(new IspNode(this, isp));
        }

        public WebNode NetworkLookup(uint address, ref int hops)
        {
            // first try to find an ISP where the address is in its network.
            var isp = _isps.FirstOrDefault(x => (address & x.Isp.SubnetMask) == (x.Address & x.Isp.SubnetMask));

            // if we've found one, then let it do a network lookup.
            if (isp != null)
            {
                return isp.NetworkLookup(address, ref hops);
            }
            
            // Now it's time to let the game's master node do a lookup.
            hops++;
            return _master.NetworkLookup(address, ref hops);
        }
    }
}