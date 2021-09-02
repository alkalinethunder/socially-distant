using System.Collections.Generic;
using Thundershock.IO;

namespace SociallyDistant.IO
{
    public class DevFs : Node
    {
        private Node _parent;

        public DevFs(Node parent)
        {
            _parent = parent;
        }

        public override bool CanDelete => false;
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => true;
        public override bool CanCreate => false;
        public override Node Parent => _parent;

        public override IEnumerable<Node> Children
        {
            get
            {
                yield return new NullNode(this);
                yield return new DebugNode(this);
            }
        }

        public override string Name => "dev";
    }
}