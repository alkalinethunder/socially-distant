using System;
using Microsoft.Xna.Framework;
using RedTeam.Core;
using RedTeam.Core.Components;
using RedTeam.Core.Config;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Gui.Elements;
using RedTeam.Core.Net;
using RedTeam.Core.SaveData;
using Thundershock;
using Thundershock.Components;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;
using Thundershock.Input;
using Thundershock.Rendering;

namespace RedTeam
{
    public sealed class Workspace : Scene
    {
        #region APP REFERENCES

        private SaveManager _saveManager;
        private ContentManager _content;
        private RedConfigManager _redConf;
        
        #endregion

        #region SCENE COMPONENTS

        private Backdrop _backdrop;
        private GuiSystem _guiSystem;
        private WindowManager _windowManager;
        private NetworkSimulation _network;
        private Shell _shell;

        #endregion
        
        #region USER INTERFACE

        private Stacker _master = new();
        private Panel _statusBG = new();
        private Stacker _statusStacker = new();
        private ConsoleControl _console = new();
        private TextBlock _playerInfo = new();
        private Stacker _workspaceStacker = new();
        private Stacker _sidebar = new();
        private Stacker _tray = new();
        private Button _settings = new();
        private Button _exit = new();
        private TextBlock _time = new();
        private Panel _bgOverlay = new();
        
        #endregion
        
        #region STATE

        private IRedTeamContext _context;
        private ColorPalette _palette;
        
        #endregion

        protected override void OnLoad()
        {
            // Camera setup.
            Camera = new Camera2D();
            
            // Grab app references.
            _saveManager = App.GetComponent<SaveManager>();
            _content = App.GetComponent<ContentManager>();
            _redConf = App.GetComponent<RedConfigManager>();
            
            // Add scene components.
            _backdrop = AddComponent<Backdrop>();
            _guiSystem = AddComponent<GuiSystem>();
            _windowManager = AddComponent<WindowManager>();
            _network = AddComponent<NetworkSimulation>();
            
            // Set up the Player Context.
            SetupPlayerContext();
            
            // Build the workspace GUI.
            BuildGui();

            // Load the redconf state.
            LoadConfig();
            
            // Style the GUI.
            StyleGui();
            
            // Start the command shell.
            StartShell();
            
            // Link the Window Manager with the GUI.
            _windowManager.AddToGuiRoot(_guiSystem);

            // Bind to configuration reloads.
            _redConf.ConfigUpdated += RedConfOnConfigUpdated;

            // Bind to settings click.
            _settings.MouseUp += SettingsOnMouseUp;
            
            base.OnLoad();
        }

        private void SettingsOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                if (!HasComponent<SettingsComponent>())
                    AddComponent<SettingsComponent>();
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _playerInfo.Text =
                $"{_context.UserName}@{_context.HostName} ({NetworkHelpers.ToIPv4String(_context.Network.LocalAddress)})";
            base.OnUpdate(gameTime);
        }

        private void RedConfOnConfigUpdated(object? sender, EventArgs e)
        {
            LoadConfig();
            StyleGui();
        }

        private void LoadConfig()
        {
            // console fonts.
            _redConf.SetConsoleFonts(_console);
            
            // Color palette.
            _palette = _redConf.GetPalette();
            _console.ColorPalette = _palette;
        }
        
        private void StartShell()
        {
            var sh = new Shell(_console, _context.Vfs, _context);
            _shell = sh;
            AddComponent(_shell);
            
            _shell.RegisterBuiltin("save", "Save the current game.", TrySave);
        }
        
        private void SetupPlayerContext()
        {
            // Get the player agent controller from Save System.
            var agent = _saveManager.GetPlayerAgent();
            
            // Create a Network Interface inside the Network Simulation for the player's
            // device.
            var nic = _network.GetNetworkInterface(agent.Device);

            // Create the player's Red Team Context.
            var ctx = new DeviceContext(agent, nic);
            
            // Store the context.
            _context = ctx;
        }

        private void BuildGui()
        {
            // Static text.
            _exit.Text = "Shut down";
            _settings.Text = "Options";
            
            // Alignments.
            _tray.VerticalAlignment = VerticalAlignment.Center;
            _playerInfo.VerticalAlignment = VerticalAlignment.Center;
            
            // Padding.
            _exit.Padding = 1;
            _settings.Padding = 1;
            _sidebar.Padding = 5;
            _statusStacker.Padding = 2;
            _tray.Padding = 1;
            
            // Fills.
            _console.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _playerInfo.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _workspaceStacker.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            // Stacker directions.
            _tray.Direction = StackDirection.Horizontal;
            _statusStacker.Direction = StackDirection.Horizontal;
            _workspaceStacker.Direction = StackDirection.Horizontal;
            
            // GUI tree
            _tray.Children.Add(_time);
            _tray.Children.Add(_settings);
            _tray.Children.Add(_exit);
            _statusStacker.Children.Add(_playerInfo);
            _statusStacker.Children.Add(_tray);
            _statusBG.Children.Add(_statusStacker);
            _workspaceStacker.Children.Add(_console);
            _workspaceStacker.Children.Add(_sidebar);
            _master.Children.Add(_statusBG);
            _master.Children.Add(_workspaceStacker);
            _bgOverlay.Children.Add(_master);
            _guiSystem.AddToViewport(_bgOverlay);
        }

        private void StyleGui()
        {
            // Status panel.
            if (_palette.BackgroundImage != null)
            {
                _statusBG.BackColor = ThundershockPlatform.HtmlColor("#222222") * 0.5f;
            }
            else
            {
                _statusBG.BackColor = ThundershockPlatform.HtmlColor("#222222");
            }
            
            // Backdrop.
            _backdrop.Texture = _palette.BackgroundImage;
            _console.DrawBackgroundImage = false;
            _bgOverlay.BackColor = _palette.GetColor(ConsoleColor.Black);
        }

        private void TrySave(IConsole console)
        {
            _saveManager.Save();
            _console.WriteLine($"&b * save successful * &B");
        }
    }
}