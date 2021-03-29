using System.Collections.Generic;
using System.Linq;
using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class RegionNode : WebNode
    {
        private MasterWebNode _master;
        private RegionNetwork _region;
        private List<IspNode> _isps = new();
        
        public RegionNetwork Region => _region;
        
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
    }
}