using DocoptNet;
using RedTeam.Connectivity;
using RedTeam.Core.Config;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Game;
using RedTeam.Core.IO;
using RedTeam.Core.SaveData;
using RedTeam.Core.Windowing;
using RedTeam.Gui.Styles;
using RedTeam.Windowing;
using Thundershock;
using Thundershock.Gui;

namespace RedTeam
{
    public class RedTeamApp : NewGameAppBase
    {
        protected override void OnPreInit()
        {
            // Global window theme.
            WindowManager.SetGlobalTheme<WhiteCarbonTheme>();
            
            // Use our own UI skin for the UI
            GuiSystem.SetDefaultStyle<HackerStyle>();

            // Register the KmsgLogOutput now so that the user can see the thundershock log in /dev/kmsg.
            Logger.AddOutput(new KmsgLogOutput());
            
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