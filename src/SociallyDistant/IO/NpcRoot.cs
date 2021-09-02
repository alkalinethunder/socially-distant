using System.Collections.Generic;
using SociallyDistant.SaveData;
using Thundershock.IO;

namespace SociallyDistant.IO
{
    public class NpcRoot : Node
    {
        private Device _device;

        public NpcRoot(Device device)
        {
            _device = device;
        }

        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => true;
        public override bool CanDelete => false;
        public override bool CanCreate => false;
        public override string Name => "/";
        public override Node Parent => null;

        public override IEnumerable<Node> Children
        {
            get
            {
                yield return new DevFs(this);
            }
        }
    }
}