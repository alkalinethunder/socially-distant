using System.Collections.Generic;
using System.IO;
using System.Linq;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.SaveData;
using Thundershock.IO;

namespace SociallyDistant.Core.IO
{
    public class HomesMount : Node
    {
        private DeviceData _device;
        private Node _parent;
        private SaveManager _saveManager;
        
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanCreate => false;
        public override bool CanList => true;
        public override bool CanDelete => false;
        public override string Name => "home";

        public override IEnumerable<Node> Children
        {
            get
            {
                var dir = _saveManager.GetHomeDirectory(_device);
                
                foreach (var user in _device.Users)
                {
                    var subdir = Path.Combine(dir, user);

                    yield return new HostDirectoryNode(this, subdir);
                }
            }
        }

        public override Node Parent => _parent;
        
        public HomesMount(Node parent, SaveManager saveManager, DeviceData deviceData)
        {
            _parent = parent;
            _device = deviceData;
            _saveManager = saveManager;
        }
    }
}