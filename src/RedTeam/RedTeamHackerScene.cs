using System;
using Microsoft.Xna.Framework;
using Thundershock.Gui;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Thundershock.IO;
using RedTeam.Components;
using RedTeam.Config;
using RedTeam.Game;
using RedTeam.Gui.Elements;
using RedTeam.IO;
using RedTeam.Net;
using RedTeam.SaveData;
using Thundershock;
using Thundershock.Rendering;

namespace RedTeam
{
    public class RedTeamHackerScene : Scene
    {
        #region GLOBAL COMPONENT REFERENCES

        private SaveManager _saveManager;
        private RedConfigManager _redConfig;
        private RiskSystem _risk;

        #endregion
        
        #region GUI ELEMENTS
        
        private ConsoleControl _console;
        private Pane _consolesPane;
        
        #endregion

        #region SCENE COMPONENTS

        private TutorialComponent _tutorial;
        private GuiSystem _guiSystem;
        private Shell _shell;
        private NetworkSimulation _netSimulation;
        private TraceTimerComponent _tracer;
        private WindowManager _windowManager;

        #endregion

        #region AGENT STATE

        private AgentController _agent;
        private NetworkInterface _nic;
        private IRedTeamContext _ctx;
        private FileSystem _fs;

        #endregion
        
        protected override void OnLoad()
        {
            // Set up the camera.   
            Camera = new Camera2D();
            Camera.ViewportWidth = 1920;
            Camera.ViewportHeight = 1080;
            
            // Get references to frequently needed global thundershock modules.
            _risk = App.GetComponent<RiskSystem>();
            _redConfig = App.GetComponent<RedConfigManager>();
            _saveManager = App.GetComponent<SaveManager>();

            // Create scene components for different portions of the game
            // that are only active during gameplay.
            _netSimulation = AddComponent<NetworkSimulation>();
            _guiSystem = AddComponent<GuiSystem>();
            _tracer = AddComponent<TraceTimerComponent>();
            _windowManager = AddComponent<WindowManager>();
            _tutorial = AddComponent<TutorialComponent>();
            
            // Build the GUI layout.
            BuildGui();
            
            // Theatrically load the game's save file.
            TheatricallyStartGame();
            
            // Initially load the console theme and make sure it is
            // re-loaded when redconf changes.
            _console.ColorPalette = _redConfig.GetPalette();
            _redConfig.ConfigUpdated += ApplyConfig;
        }

        private void ApplyConfig(object? sender, EventArgs e)
        {
            _console.ColorPalette = _redConfig.GetPalette();
        }

        private void StartShell()
        {
            // kill the tutorial manager
            RemoveComponent(_tutorial);
            
            _agent = _saveManager.GetPlayerAgent();
            _nic = _netSimulation.GetNetworkInterface(_agent.Device);
            _ctx = new DeviceContext(_agent, _nic);
            _fs = _ctx.Vfs;
            _shell = new Shell(_console, _fs, _ctx);
            AddComponent(_shell);
        }
        
        
        protected override void OnUpdate(GameTime gameTime)
        {
            _console.ColorPalette.PanicMode = _risk.IsBeingTraced;
            
        }

        private void BuildGui()
        {
            // redterm shell.
            _console = new ConsoleControl();
            
            // Create the consoles pane.
            // TODO: Multiple consoles.
            _consolesPane = _windowManager.CreatePane("TERMINAL");
            _consolesPane.Content.Add(_console);
            
            // add the console pane to the GUI.
            // TODO: actually not shitty layout.
            _guiSystem.AddToViewport(_consolesPane);

        }

        private void TheatricallyStartGame()
        {
            _console.WriteLine(" * checking for redteam os container *");
            if (_saveManager.IsSaveAvailable)
            {
                _saveManager.LoadGame();
                StartShell();
            }
            else
            {
                _saveManager.NewGame();
                _console.WriteLine(" * container image not found *");
                _tutorial.TutorialCompleted += StartShell;
                _tutorial.Begin(_saveManager, _console);
            }
        }
    }
}