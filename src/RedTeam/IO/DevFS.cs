using System.Collections.Generic;

namespace RedTeam.IO
{
    public class DevFS : Node
    {
        private Node _parent;

        public DevFS(Node parent)
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
            }
        }

        public override string Name => "dev";
    }
}