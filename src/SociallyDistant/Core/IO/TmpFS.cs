using System.Collections.Generic;
using System.Linq;
using Thundershock.IO;

namespace SociallyDistant.Core.IO
{
    public class TmpFS : Node
    {
        private Node _parent;
        
        public override bool CanDelete => false;
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => true;
        public override bool CanCreate => true;
        public override Node Parent => _parent;

        public override IEnumerable<Node> Children
        {
            get
            {
                return Enumerable.Empty<Node>();
            }
        }

        public override string Name => "tmp";

        public TmpFS(Node parent)
        {
            _parent = parent;
        }
    }
}