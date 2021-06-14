using DocoptNet;
using RedTeam.Connectivity;
using RedTeam.Core.Config;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Game;
using RedTeam.Core.IO;
using RedTeam.Core.SaveData;
using RedTeam.Gui.Styles;
using Thundershock;
using Thundershock.Gui;

namespace RedTeam
{
    public class RedTeamApp : App
    {
        protected override void OnPreInit()
        {
            // We use the GUI system A LOT, so rather than having to set the game style on each scene
            // we'll do it now.
            GuiSystem.SetDefaultStyle<HackerStyle>();
            
            // Register the KmsgLogOutput now so that the user can see the thundershock log in /dev/kmsg.
            Logger.AddOutput(new KmsgLogOutput());
            
            // Announcement manager will pull community updates from aklnthndr.dev
            RegisterComponent<AnnouncementManager>();
            
            base.OnPreInit();
        }

        protected override void OnInit()
        {
            // Window Title
            Window.Title = "Michael VanOverbeek's RED TEAM";
            
            // register red team components
            RegisterComponent<ContentManager>();
            RegisterComponent<RedConfigManager>();
            RegisterComponent<SaveManager>();
            RegisterComponent<RiskSystem>();
        }

        protected override void OnLoad()
        {
            // Users are able to disable the in-game splash screen should they find it
            // annoying.
            //
            // This is where that option is honoured.
            if (GetComponent<RedConfigManager>().ActiveConfig.SkipIntro)
            {
                // Skip straight to the console scene.
                LoadScene<MainMenu>();
            }
            else
            {
                // load the intro scene!
                LoadScene<Intro>();
            }
        }
    }
}