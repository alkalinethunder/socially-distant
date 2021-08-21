using System;
using System.IO;
using Thundershock;
using Thundershock.Core;
using Thundershock.Gui;

namespace SociallyDistant
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AssertCriticalFileExists("Assets", "osicons.pak");

            AssetManager.AddThirdPartyPak("icon", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "osicons.pak"));
            
            EntryPoint.RegisterApp<ContentEditorApp>("editor");
            EntryPoint.Run<RedTeamApp>(args);
        }

        private static void AssertCriticalFileExists(params string[] path)
        {
            var relative = Path.Combine(path);
            var full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);

            if (!File.Exists(full))
            {
                DialogBox.ShowError("Socially Distant",
                    "Socially Distant cannot start because a critical file is missing from the installation. That file's expected path is shown below. You will need to reinstall the game." +
                    Environment.NewLine + Environment.NewLine + full);
                
                Environment.Exit(-1);
            }
        }
    }
}
