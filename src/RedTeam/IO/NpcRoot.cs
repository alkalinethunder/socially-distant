using System.Collections.Generic;
using RedTeam.SaveData;
using Thundershock.IO;

namespace RedTeam.IO
{
    public class NpcRoot : Node
    {
        private Device _device;
        private string _username;
        private SaveManager _saveManager;

        public NpcRoot(SaveManager saveManager, Device device, string username)
        {
            _saveManager = saveManager;
            _device = device;
            _username = username;
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
                yield return new DevFS(this);
                yield return new RedEtc(this, _device);
            }
        }
    }
}