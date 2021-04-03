using RedTeam.Net;
using Thundershock.IO;

namespace RedTeam
{
    public interface IRedTeamContext
    {
        public NetworkInterface Network { get; }
        public FileSystem Vfs { get; }
        string UserName { get; }
        string HostName { get; }
        string HomeDirectory { get; }
        string Terminal { get; }
        string Shell { get; }
        string WindowManager { get; }
        string DesktopEnvironment { get; }

        void ShutDown();
    }
}