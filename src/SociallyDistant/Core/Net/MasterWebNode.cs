using System.Collections.Generic;
using System.Linq;
using SociallyDistant.Core.SaveData;

namespace SociallyDistant.Core.Net
{
    public class MasterWebNode : WebNode
    {
        private List<RegionNode> _regions = new List<RegionNode>();

        public override IEnumerable<WebNode> ConnectedNodes => _regions;

        public override WebNodeType Type => WebNodeType.Master;

        public override string Name => "<MASTER>";
        public override uint Address => 0x0;
        
        public IEnumerable<WebNode> CollapseNodes()
        {
            // UNLEASH...
            // YOUR INNER...
            // PHILIP ADAMS!
            yield return this;
            foreach (var region in _regions)
            {
                yield return region;
                foreach (var rChild in region.ConnectedNodes)
                {
                    if (rChild is IspNode isp)
                    {
                        yield return isp;
                        foreach (var iChild in isp.ConnectedNodes)
                        {
                            if (iChild is NetworkNode net)
                            {
                                yield return net;
                                foreach (var nChild in net.ConnectedNodes)
                                {
                                    if (nChild is DeviceNode dev)
                                        yield return dev;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public RegionNode GetRegionNode(RegionNetwork region)
        {
            return _regions.First(x => x.Region.Id == region.Id);
        }
        
        public RegionNode AddRegion(RegionNetwork region)
        {
            var node = new RegionNode(this, region);
            _regions.Add(node);
            return node;
        }

        public WebNode NetworkLookup(uint address, ref int hops)
        {
            // try to find a region with a matching subnet
            var reg = _regions.FirstOrDefault(x =>
                (address & x.Region.SubnetMask) == (x.Address & x.Region.SubnetMask));

            // do a lookup in that area of the world.
            if (reg != null)
            {
                hops++;
                return reg.NetworkLookup(address, ref hops);
            }

            // The requested host doesn't exist.
            return null;
        }
    }
}