﻿using SociallyDistant.ContentEditor;
using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
using Thundershock;

namespace SociallyDistant
{
    public class ContentEditorApp : NewGameAppBase
    {
        protected override void OnPreInit()
        {
            // module manager stuff
            ModuleManager.Initialize();

            // register the editor console
            var econsole = RegisterComponent<EditorConsole>();
            Logger.AddOutput(econsole);

            base.OnPreInit();
        }

        protected override void OnInit()
        {
            Window.Title = "Socially Distant Editor";

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