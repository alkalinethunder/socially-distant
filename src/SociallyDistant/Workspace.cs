using System;
using SociallyDistant.Core;
using SociallyDistant.Core.Components;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.Net;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant
{
    public sealed class Workspace : Scene
    {
        #region APP REFERENCES

        private SaveManager _saveManager;
        private RedConfigManager _redConf;
        
        #endregion

        #region SCENE COMPONENTS

        private WindowManager _windowManager;
        private Shell _shell;

        #endregion
        
        #region USER INTERFACE

        private Stacker _master = new();
        private Panel _statusBg = new();
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

        private TimeSpan _uptime;
        private TimeSpan _frameTime;
        private IRedTeamContext _context;
        private ColorPalette _palette;
        
        #endregion

        #region WINDOWS

        private SettingsWindow _settingsWindow;

        #endregion
        
        #region PROPERTIES

        public TimeSpan Uptime => _uptime;
        public TimeSpan FrameTime => _frameTime;

        #endregion
        
        protected override void OnLoad()
        {
            // Grab app references.
            _saveManager = Game.GetComponent<SaveManager>();
            _redConf = Game.GetComponent<RedConfigManager>();
            
            // Build the workspace GUI.
            BuildGui();

            // Load the redconf state.
            LoadConfig();
            
            // Style the GUI.
            StyleGui();
            
            // Start the command shell.
            StartShell();
            
            // Bind to configuration reloads.
            _redConf.ConfigUpdated += RedConfOnConfigUpdated;

            // Bind to settings click.
            _settings.MouseUp += SettingsOnMouseUp;

            // Window manager.
            _windowManager = RegisterSystem<WindowManager>();
            
            base.OnLoad();
        }

        private void SettingsOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                if (_settingsWindow == null)
                {
                    _settingsWindow = _windowManager.OpenWindow<SettingsWindow>();
                    _settingsWindow.WindowClosed += SettingsWindowOnWindowClosed;
                }
            }
        }

        private void SettingsWindowOnWindowClosed(object sender, EventArgs e)
        {
            _settingsWindow = null;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _frameTime = gameTime.ElapsedGameTime;
            _uptime = gameTime.TotalGameTime;
            
            base.OnUpdate(gameTime);

            // TODO: Somehow get Thundershock to do this. Maybe there's a way to set up
            // the shell with the ECS somehow? I mean it's not an entity, more of a system,
            // but hmmm.
            _shell.Update(gameTime);
        }

        private void RedConfOnConfigUpdated(object sender, EventArgs e)
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
            // Start the game's simulation.
            var simulation = RegisterSystem<Simulation>();
            
            // Register the shell as a system.
            _shell = RegisterSystem<Shell>();

            // Attach a shell to the player entity.
            var playerEntity = simulation.GetPlayerEntity();
            Registry.AddComponent(playerEntity, (IConsole) _console);
            Registry.AddComponent(playerEntity, new ShellStateComponent
            {
                UserId = 1 // uses the player's  normal user account instead of root.
            });
        }
        
        private void SetupPlayerContext()
        {
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

            Gui.AddToViewport(_console);
        }

        private void StyleGui()
        {
            // Status panel.
            if (_palette.BackgroundImage != null)
            {
                _statusBg.BackColor = Color.FromHtml("#222222") * 0.5f;
            }
            else
            {
                _statusBg.BackColor = Color.FromHtml("#222222");
            }
            
            // Backdrop.
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