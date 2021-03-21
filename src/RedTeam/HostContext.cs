using System;
using System.IO;
using RedTeam.IO;

namespace RedTeam
{
    public class HostContext : IRedTeamContext
    {
        public string UserName => Environment.UserName;
        public string HostName => System.Net.Dns.GetHostName();

        public string HomeDirectory
        {
            get
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                if (RedTeamPlatform.IsPlatform(Platform.Windows))
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
    }
}