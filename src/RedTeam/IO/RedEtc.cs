using System.Collections.Generic;
using RedTeam.SaveData;
using Thundershock.IO;

namespace RedTeam.IO
{
    public class RedEtc : Node
    {
        private Node _parent;
        private Device _device;

        public RedEtc(Node parent, Device device)
        {
            _parent = parent;
            _device = device;
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
                yield return new HostnameNode(this, _device);
            }
        }
        public override string Name => "etc";
    }
}