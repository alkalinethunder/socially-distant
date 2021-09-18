using System;
using System.IO;
using SociallyDistant.Core;
using SociallyDistant.Editor;
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
            // Check for required PAK data files and exit with error if they're not found.
            AssertCriticalFileExists("Assets", "osicons.pak");
            AssertCriticalFileExists("Assets", "world-base.pak");

            // Load in the OS Icons package.
            AssetManager.AddThirdPartyPak("icon", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "osicons.pak"));
            
            // Register the entry point for the Socially Distant Editor.
            EntryPoint.RegisterApp<ContentEditorApp>("editor");
            
            // Run the game.
            EntryPoint.Run<SociallyDistantApp>(args);
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
