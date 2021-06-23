using System;
using RedTeam.Core;
using RedTeam.Core.Net;
using RedTeam.Core.SaveData;
using Thundershock;
using Thundershock.Core;
using Thundershock.IO;

namespace RedTeam
{
    public class DeviceContext : IRedTeamContext
    {
        private FileSystem _vfs;
        private AgentController _agent;
        private NetworkInterface _nic;
        private Workspace _workspace;
        
        public bool IsGraphical => true;
        
        public DeviceContext(Workspace workspace, AgentController agent, NetworkInterface nic)
        {
            _workspace = workspace;
            _agent = agent;
            _nic = nic;
        }

        public TimeSpan Uptime => _workspace.Uptime;
        public TimeSpan FrameTime => _workspace.FrameTime;
        public int ScreenWidth => (int) _workspace.Screen.Width;
        public int ScreenHeight => (int) _workspace.Screen.Height;
        
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