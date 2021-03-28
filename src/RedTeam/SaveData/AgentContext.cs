using RedTeam.IO;
using Thundershock.IO;

namespace RedTeam.SaveData
{
    public class AgentContext : IRedTeamContext
    {
        private AgentController _controller;
        private FileSystem _vfs;
        
        public AgentContext(AgentController controller)
        {
            _controller = controller;
        }

        public FileSystem Vfs
        {
            get
            {
                if (_vfs != null)
                    return _vfs;

                _vfs = _controller.CreateVfs();
                return _vfs;
            }
        }
        
        public string UserName => _controller.UserName;
        public string HostName => _controller.HostName;
        public string HomeDirectory => PathUtils.Combine("home", UserName);
        public string Terminal => "redterm";
        public string Shell => "redsh";
        public string WindowManager => "redwm";
        public string DesktopEnvironment => "thundershock";
    }
}