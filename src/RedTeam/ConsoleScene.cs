using Microsoft.Xna.Framework;
using RedTeam.Gui;
using System.Collections.Generic;
using RedTeam.IO;

namespace RedTeam
{
    public class ConsoleScene : Scene
    {
        private GuiSystem _guiSystem;
        private ConsoleControl _console;
        private Shell _shell;
        
        protected override void OnLoad()
        {
            _guiSystem = AddComponent<GuiSystem>();
            _console = new ConsoleControl();
            _guiSystem.AddToViewport(_console);
            _shell = new Shell(_console, FileSystem.FromHostOS(), new HostContext());
            AddComponent(_shell);
        }
    }
}