using System.Collections.Generic;
using System.IO;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.IO;
using SociallyDistant.Core.SaveData;
using Thundershock.IO;

namespace SociallyDistant.Core.Game
{
    public class VOSRootNode : Node
    {
        private SaveManager _saveManager;
        private DeviceData _device;
        
        public override bool CanDelete => false;
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => true;
        public override bool CanCreate => false;
        public override Node Parent => null;
        public override string Name => "/";

        public override IEnumerable<Node> Children
        {
            get
            {
                var deviceHome = _saveManager.GetHomeDirectory(_device);
                var rootHome = Path.Combine(deviceHome, "root");

                yield return new RedEtc(this, _device);
                yield return new DevFs(this);
                yield return new TmpFS(this);
                yield return new HomesMount(this, _saveManager, _device);
                yield return new HostDirectoryNode(this, rootHome);
            }
        }
        
        public VOSRootNode(SaveManager saveManager, DeviceData saveDevice)
        {
            _saveManager = saveManager;
            _device = saveDevice;
        }
    }
}