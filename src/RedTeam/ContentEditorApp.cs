using RedTeam.ContentEditor;
using Thundershock;

namespace RedTeam
{
    public class ContentEditorApp : App
    {
        protected override void OnPreInit()
        {
            // register the editor console
            var econsole = RegisterComponent<EditorConsole>();
            Logger.AddOutput(econsole);

            base.OnPreInit();
        }

        protected override void OnInit()
        {
            Window.Title = "Michael VanOverbeek's RED TEAM - Content Editor";
            base.OnInit();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            LoadScene<ContentEditorScene>();
        }
    }
}