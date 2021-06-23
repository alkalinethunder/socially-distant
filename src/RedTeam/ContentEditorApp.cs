using RedTeam.ContentEditor;
using RedTeam.Core.ContentEditors;
using Thundershock;
using Thundershock.Gui;

namespace RedTeam
{
    public class ContentEditorApp : NewGameAppBase
    {
        protected override void OnPreInit()
        {
            // register the editor console
            var econsole = RegisterComponent<EditorConsole>();
            Logger.AddOutput(econsole);

            base.OnPostInit();
        }

        protected override void OnInit()
        {
            Window.Title = "Michael VanOverbeek's RED TEAM - Content Editor";

            RegisterComponent<ContentManager>();
            
            base.OnInit();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            LoadScene<ContentEditorScene>();
        }
    }
}