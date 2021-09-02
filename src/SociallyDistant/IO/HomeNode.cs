using System.Collections.Generic;
using System.IO;
using System.Linq;
using SociallyDistant.Core.SaveData;
using Thundershock;
using Thundershock.IO;

namespace SociallyDistant.Core.IO
{
    public class HomeNode : Node
    {
        private Node _parent;

        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanCreate => true;
        public override bool CanDelete => false;
        public override bool CanExecute => false;
        public override bool CanList => true;

        public override Node Parent => _parent;

        public override IEnumerable<Node> Children
        {
            get
            {
                return Enumerable.Empty<Node>();
            }
        }

        public override string Name => "user";
        
        public HomeNode(Node parent)
        {
            _parent = parent;
        }
    }
}