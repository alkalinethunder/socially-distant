using System.Collections.Generic;
using System.Linq;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class MasterWebNode : WebNode
    {
        private List<RegionNode> _regions = new List<RegionNode>();

        public override IEnumerable<WebNode> ConnectedNodes => _regions;

        public override WebNodeType Type => WebNodeType.Master;

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
        
        
    }
}