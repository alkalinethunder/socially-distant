using System.IO;
using SociallyDistant.Connectivity;
using SociallyDistant.Core;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.IO;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Windowing;
using SociallyDistant.Gui.Styles;
using SociallyDistant.Windowing;
using Thundershock;
using Thundershock.Gui;

namespace SociallyDistant
{
    public class RedTeamApp : NewGameAppBase
    {
        protected override void OnPreInit()
        {
            // Create the skindata  folder if it doesn't exist. This allows us to load theme assets.
            var skindata = Path.Combine(ThundershockPlatform.LocalDataPath, "skindata");
            if (!Directory.Exists(skindata))
                Directory.CreateDirectory(skindata);
            
            // Mount the skindata folder in the asset manager.
            AssetManager.AddDirectory("skindata", skindata);
            
            // module manager init
            ModuleManager.Initialize();

            Window.Title = "Socially Distant";
        
            // Global window theme.
            WindowManager.SetGlobalTheme<WhiteCarbonTheme>();
            
            // Use our own UI skin for the UI
            GuiSystem.SetDefaultStyle<HackerStyle>();
            
            // Announcement manager will pull community updates from aklnthndr.dev
            RegisterComponent<AnnouncementManager>();
            
            base.OnPreInit();
        }

        protected override void OnInit()
        {
            // register red team components
            RegisterComponent<ContentManager>();
            RegisterComponent<RedConfigManager>();
            RegisterComponent<SaveManager>();
        }

        protected override void OnLoad()
        {
            // Launch us straight into the v-OS boot screen.
            LoadScene<Intro>();
        }
    }
}