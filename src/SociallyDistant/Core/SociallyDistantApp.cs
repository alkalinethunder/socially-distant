using System.IO;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Editor;
using SociallyDistant.Gui.Styles;
using SociallyDistant.Gui.Windowing;
using SociallyDistant.Modding;
using SociallyDistant.Online.CommunityAnnouncements;
using SociallyDistant.Scenes;
using Thundershock;
using Thundershock.Core;
using Thundershock.Gui;

namespace SociallyDistant.Core
{
    public class SociallyDistantApp : GameApplication
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
            
            base.OnPreInit();
        }
        
        protected override void OnLoad()
        {
            // Launch us straight into the v-OS boot screen.
            LoadScene<Intro>();
        }

        protected override void BeforeFrame(GameTime gameTime)
        {
            AnnouncementManager.Instance.OnUpdate(gameTime);
            SaveManager.Instance.OnUpdate(gameTime);
        }
    }
}