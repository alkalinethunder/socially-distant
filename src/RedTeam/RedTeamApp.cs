using RedTeam.Config;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam
{
    public class RedTeamApp : App
    {
        protected override void OnInit()
        {
            // Window Title
            Window.Title = "Michael VanOverbeek's RED TEAM";
            
            // register red team components
            RegisterComponent<SaveManager>();
            RegisterComponent<RedConfigManager>();
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
                LoadScene<ConsoleScene>();
            }
            else
            {
                // load the intro scene!
                LoadScene<Intro>();
            }
        }
    }
}