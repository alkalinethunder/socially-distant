using RedTeam.IO;
using RedTeam.SaveData;
using Thundershock.IO;

namespace RedTeam.Net
{
    public class HackContext : IRedTeamContext
    {
        private Device _device;
        private NetworkInterface _nic;
        private SaveManager _SaveManager;
        private FileSystem _vfs;
        private HackStartInfo _hack;
        
        public HackContext(HackStartInfo hack, SaveManager saveManager, Device device, NetworkInterface nic)
        {
            _hack = hack;
            _SaveManager = saveManager;
            _nic = nic;
            _device = device;
        }

        public NetworkInterface Network => _nic;

        public FileSystem Vfs
        {
            get
            {
                if (_vfs == null)
                {
                    var root = new NpcRoot(_SaveManager, _device, "root");

                    _vfs = FileSystem.FromNode(root);
                }

                return _vfs;
            }
        }

        public string UserName => "root";
        public string HostName => _device.HostName;
        public string HomeDirectory => "/root";
        public string Terminal => "ssh";
        public string Shell => "redrev";
        public string WindowManager => "";
        public string DesktopEnvironment => "";

        public void ShutDown()
            => _hack.Finish();
    }
}