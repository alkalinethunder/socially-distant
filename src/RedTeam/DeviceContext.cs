using RedTeam.Core;
using RedTeam.Core.Net;
using RedTeam.Core.SaveData;
using Thundershock;
using Thundershock.IO;

namespace RedTeam
{
    public class DeviceContext : IRedTeamContext
    {
        private FileSystem _vfs;
        private AgentController _agent;
        private NetworkInterface _nic;

        public DeviceContext(AgentController agent, NetworkInterface nic)
        {
            _agent = agent;
            _nic = nic;
        }

        public NetworkInterface Network => _nic;

        public FileSystem Vfs
        {
            get
            {
                if (_vfs == null)
                    _vfs = _agent.CreateVfs();
                return _vfs;
            }
        }

        public string UserName => _agent.UserName;
        public string HostName => _agent.HostName;
        public string HomeDirectory => PathUtils.Combine("home", UserName);
        public string Terminal => "redterm";
        public string Shell => "redsh";
        public string WindowManager => "redwm";
        public string DesktopEnvironment => "thundershock";

        public void ShutDown()
            => EntryPoint.CurrentApp.Exit();
    }
}