using System;
using System.IO;
using RedTeam.IO;
using RedTeam.Net;
using Thundershock;
using Thundershock.IO;

namespace RedTeam
{
    public class HostContext : IRedTeamContext
    {
        private FileSystem _vfs;

        public NetworkInterface Network => null;
        public FileSystem Vfs
        {
            get
            {
                if (_vfs == null)
                    _vfs = FileSystem.FromHostOS();

                return _vfs;
            }
        }
        public string Shell => "redsh 0.1";
        public string Terminal => "redterm";
        public string WindowManager => "redwm";
        public string DesktopEnvironment => "Redteam GUI";
        public string UserName => Environment.UserName;
        public string HostName => System.Net.Dns.GetHostName();

        public string HomeDirectory
        {
            get
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                if (ThundershockPlatform.IsPlatform(Platform.Windows))
                {
                    // windows to vfs path separators
                    home = home.Replace(Path.DirectorySeparatorChar.ToString(), PathUtils.Separator);
                    
                    // get the drive letter
                    var letter = home.Substring(0, home.IndexOf(PathUtils.Separator));
                    
                    // remove the drive letter from the vfs path.
                    home = home.Substring(letter.Length);
                    
                    // drive letter to vfs drive folder
                    letter = letter.ToLower().Replace(":", "");
                    
                    // add letter back to vfs path
                    home = home.Insert(0, PathUtils.Separator + letter);
                }

                return home;
            }
        }

        public void ShutDown() => EntryPoint.CurrentApp.Exit();
    }
}