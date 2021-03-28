using RedTeam.Config;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam
{
    public class RedTeamApp : App
    {
        protected override void OnInit()
        {
            // register red team components
            RegisterComponent<SaveManager>();
            RegisterComponent<RedConfigManager>();
        }

        protected override void OnLoad()
        {
            // load the intro scene!
            LoadScene<Intro>();
        }
    }
}