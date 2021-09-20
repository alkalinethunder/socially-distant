using SociallyDistant.Editor.Scenes;
using SociallyDistant.Modding;
using Thundershock;

namespace SociallyDistant.Editor
{
    public class ContentEditorApp : GameApplication
    {
        protected override void OnPreInit()
        {
            // module manager stuff
            ModuleManager.Initialize();

            base.OnPreInit();
        }

        protected override void OnInit()
        {
            Window.Title = "Socially Distant Editor";

            base.OnInit();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            LoadScene<ContentEditorScene>();
        }
    }
}