using System;
using Microsoft.Xna.Framework;
using RedTeam.Gui;
using System.Collections.Generic;
using RedTeam.Config;
using RedTeam.IO;

namespace RedTeam
{
    public class ConsoleScene : Scene
    {
        private GuiSystem _guiSystem;
        private ConsoleControl _console;
        private Shell _shell;
        private ConfigurationManager _config;
        
        protected override void OnLoad()
        {
            _config = Game.GetComponent<ConfigurationManager>();
            _config.ConfigurationLoaded += ApplyConfig;

            _guiSystem = AddComponent<GuiSystem>();
            _console = new ConsoleControl();
            _guiSystem.AddToViewport(_console);

            _console.WriteLine(" * checking for proprietary content updates on github *");
            _console.WriteLine(
                "&2#c&b&u/!\\&U THIS REQUIRES INTERNET ACCESS IF A LOCAL MANIFEST ISN'T FOUND &u/!\\&U&0");
            
            _console.ColorPalette = _config.GetRedTermPalette();

            StartShell();
        }
        
        private void StartShell()
        {
            _console.Clear();
            
            _shell = new Shell(_console, FileSystem.FromHostOS(), new HostContext());
            AddComponent(_shell);
        }
        
        private void ApplyConfig(object sender, EventArgs e)
        {
            _console.ColorPalette = _config.GetRedTermPalette();
        }
    }
}