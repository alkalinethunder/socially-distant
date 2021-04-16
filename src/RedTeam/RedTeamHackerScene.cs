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
using Thundershock.Components;
using Thundershock.Rendering;
using Thundershock.Gui.Elements;

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

        private StatusPanel _mainStatus;
        private Stacker _masterLayout;
        private Stacker _sidebar = new();
        private Stacker _slaveStacker = new();
        private ConsoleControl _console;
        private Pane _contractPanel;
        private TextBlock _contractObjName = new();
        private TextBlock _contractDesc = new();
        private Panel _windowArea = new();
        
        #endregion

        #region SCENE COMPONENTS

        private Backdrop _backdrop; // easy wallpaper rendering :D
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
        
        #region ANIMATION

        private Color _sysColor;
        private double _pulseLength;
        private double _pulse;
        
        #endregion

        public WindowManager WindowManager => _windowManager;
        
        protected override void OnLoad()
        {
            // Set up the camera.   
            Camera = new Camera2D();
            Camera.ViewportWidth = 1920;
            Camera.ViewportHeight = 1080;
            
            // Background image
            _backdrop = AddComponent<Backdrop>();
            
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
            _backdrop.Texture = _console.ColorPalette.BackgroundImage;
            _redConfig.ConfigUpdated += ApplyConfig;
            
            // Pulse during panics.
            _tracer.PanicPulse += PanicPulse;
        }

        private void PanicPulse(double time)
        {
            _pulseLength = time;
            _pulse = _pulseLength;
        }

        private void ApplyConfig(object? sender, EventArgs e)
        {
            _console.ColorPalette = _redConfig.GetPalette();
            _backdrop.Texture = _console.ColorPalette.BackgroundImage;
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
            if (_pulseLength > 0)
            {
                var percent = MathHelper.Clamp((float) (_pulse / _pulseLength), 0, 1);
                _sysColor = Color.Lerp(_console.ColorPalette.DefaultWindowColor,
                    _console.ColorPalette.PanicWindowColor, percent);
            }
            else
            {
                _sysColor = _console.ColorPalette.DefaultWindowColor;
            }

            if (_pulse >= 0)
            {
                _pulse -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            
            _console.ColorPalette.PanicMode = _risk.IsBeingTraced;

            _mainStatus.FrameRate = $"{Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds)} fps";
            if (_ctx != null)
            {
                _mainStatus.HostText = $"{_ctx.UserName}@{_ctx.HostName}";
            }
            else
            {
                _mainStatus.HostText = "root@localhost";
            }

            _mainStatus.Color = _sysColor;
            _contractPanel.BorderColor = _sysColor;
        }

        private void BuildGui()
        {
            // Master layout.
            _masterLayout = new Stacker();
            
            // Status panel.
            _mainStatus = new StatusPanel(this);

            // redterm shell.
            _console = new ConsoleControl();
            _console.DrawBackgroundImage = false; // this lets us draw the image ourselves.
            
            // Contracts panel.
            _contractPanel = _windowManager.CreatePane("CONTRACT");
            
            // Add the status panel followed by window panels.
            _masterLayout.Children.Add(_mainStatus);
            _masterLayout.Children.Add(_windowArea);

            _windowArea.Children.Add(_slaveStacker);
            
            // slave stacker is horizontal
            _slaveStacker.Direction = StackDirection.Horizontal;
            
            // sidebar has a minimum width.
            _sidebar.FixedWidth = 300;
            
            // add the console and sidebar panels
            _slaveStacker.Children.Add(_console);
            _slaveStacker.Children.Add(_sidebar);
            
            // Sidebar elements.
            _sidebar.Children.Add(_contractPanel);
            
            // fix the layout of the consoles window.
            _console.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _windowArea.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            // contracts info
            _contractObjName.Text = "This is the CONTRACT PANE";
            _contractDesc.Text =
                "It's not yet implemented, but it will display information about the current Red Team Contract objective.";
            _contractObjName.Color = Color.White;
            _contractDesc.Color = Color.White;
            _contractObjName.WrapMode = TextWrapMode.WordWrap;
            _contractDesc.WrapMode = TextWrapMode.WordWrap;
            
            
            var contractPanel = new Panel();
            contractPanel.BackColor = Color.Black;
            var contractStacker = new Stacker();
            contractPanel.Children.Add(contractStacker);
            contractStacker.Children.Add(_contractObjName);
            contractStacker.Children.Add(_contractDesc);
            _contractPanel.Content.Add(contractPanel);

            _windowManager.SetWindowLayerElement(_windowArea);
            
            // Master layout gets added to the screen.
            _guiSystem.AddToViewport(_masterLayout);
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