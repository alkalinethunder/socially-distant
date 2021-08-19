using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using SociallyDistant.Core;
using SociallyDistant.Core.Components;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.Net;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Social;
using SociallyDistant.Core.Windowing;
using StbTrueTypeSharp;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Ecs;
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

        // main UI
        private Stacker _infoLeft = new();
        private Panel _infoBanner = new();
        private Stacker _infoMaster = new();
        private Stacker _infoProfileCard = new();
        private Picture _playerAvatar = new();
        private TextBlock _playerName = new();
        private Stacker _playerInfoStacker = new();
        private Stacker _infoRight = new();
        private Button _settings = new();
        private ConsoleControl _console = new();
        private Panel _terminalsPanel = new();
        private Panel _displaysPanel = new();
        private Panel _sidePanel = new();

        // notification UI
        private Panel _notificationBanner = new();
        private TextBlock _noteTitle = new();
        private TextBlock _noteMessage = new();
        private WrapPanel _noteButtonWrapper = new();
        private Picture _noteIcon = new();
        private Stacker _noteStacker = new();
        private Stacker _noteInfoStacker = new();
        private Mailbox _playerMailbox;
        
        #endregion
        
        #region STATE

        private int _noteState = 0;
        private float _noteTransition = 0;
        private double _noteTimer = 0;
        private bool _noteAutoDismiss = false;
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
            // Turn off FXAA.
            PrimaryCameraSettings.EnableFXAA = false;
            
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
            
            // Window manager.
            _windowManager = RegisterSystem<WindowManager>();
            
            base.OnLoad();

            _settings.MouseUp += SettingsOnMouseUp;
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
            // Try to find the player mailbox if we lack it.
            if (_playerMailbox == null)
            {
                var playerEntities = Registry.View<PlayerState>();
                if (playerEntities.Any())
                {
                    var first = playerEntities.First();
                    var mailbox = Registry.GetComponent<Mailbox>(first);
                    _playerMailbox = mailbox;
                }
            }
            
            // Check for in-coming email messages.
            if (_playerMailbox != null)
            {
                ReadMail();
            }
            
            _frameTime = gameTime.ElapsedGameTime;
            _uptime = gameTime.TotalGameTime;

            _playerName.Text = _saveManager.CurrentGame.PlayerName;

            if (_noteState == 0)
            {
                if (NotificationManager.TryGetNotification(out var note))
                {
                    _noteTitle.Text = note.Title;
                    _noteMessage.Text = note.Message;
                    _noteIcon.Image = note.Icon;

                    _noteIcon.Visibility = note.Icon == null ? Visibility.Collapsed : Visibility.Visible;

                    _noteButtonWrapper.Children.Clear();

                    _noteTransition = 0;
                    _noteTimer = note.Time;
                    _noteAutoDismiss = _noteTimer > 0;

                    if (!_noteAutoDismiss && !note.Actions.Any())
                    {
                        note.AddButton("OK");
                    }
                    
                    foreach (var key in note.Actions.Keys)
                    {
                        var action = note.Actions[key];
                        
                        var btn = new Button();
                        btn.Text = key;
                        btn.MouseUp += (o, a) =>
                        {
                            if (_noteState == 2)
                            {
                                if (a.Button == MouseButton.Primary)
                                {
                                    action?.Invoke();

                                    _noteState = 3;
                                }
                            }
                        };

                        btn.Padding = new Padding(0, 1, 2, 1);

                        _noteButtonWrapper.Children.Add(btn);
                    }

                    _noteState = 1;
                    Gui.AddToViewport(_notificationBanner);
                }
            }
            
            switch (_noteState)
            {
                case 1:
                    _noteTransition =
                        MathHelper.Clamp(_noteTransition + (float) gameTime.ElapsedGameTime.TotalSeconds * 4, 0, 1);

                    _notificationBanner.Opacity = _noteTransition;
                    _notificationBanner.ViewportPosition = new Vector2(0, 0 - (200 * (1 - _noteTransition)));

                    if (_noteTransition >= 1)
                    {
                        _noteState++;
                    }
                    
                    break;
                case 2:
                    if (_noteAutoDismiss)
                    {
                        _noteTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                        if (_noteTimer <= 0)
                        {
                            _noteState++;
                        }
                    }
                    break;
                case 3:
                    _noteTransition =
                        MathHelper.Clamp(_noteTransition - (float) gameTime.ElapsedGameTime.TotalSeconds * 4, 0, 1);

                    _notificationBanner.Opacity = _noteTransition;
                    _notificationBanner.ViewportPosition = new Vector2(0, 0 - (200 * (1 - _noteTransition)));

                    if (_noteTransition <= 0)
                    {
                        _noteState = 0;
                        _notificationBanner.RemoveFromParent();
                    }

                    break;
            }
            
            base.OnUpdate(gameTime);
        }

        private void ReadMail()
        {
            if (_playerMailbox.TryGetUnreadMessage(out var unread))
            {
                var note = NotificationManager.CreateNotification("Email received.", unread.Message.Subject, 5);
                    
                note.AddButton("View"); // TODO: mail open action.
                note.AddButton("Dismiss");
            }
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
            
            // With the simulation started, we can start Mailer.
            RegisterSystem<MailboxManager>();

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
        
        private void BuildGui()
        {
            _noteInfoStacker.Children.Add(_noteTitle);
            _noteInfoStacker.Children.Add(_noteMessage);
            _noteInfoStacker.Children.Add(_noteButtonWrapper);
            _noteStacker.Children.Add(_noteIcon);
            _noteStacker.Children.Add(_noteInfoStacker);
            _notificationBanner.Children.Add(_noteStacker);
            
            _playerInfoStacker.Children.Add(_playerName);
            
            _infoProfileCard.Children.Add(_playerAvatar);
            _infoProfileCard.Children.Add(_playerInfoStacker);

            _infoRight.Children.Add(_settings);
            _infoRight.Children.Add(_infoProfileCard);

            _infoMaster.Children.Add(_infoLeft);
            _infoMaster.Children.Add(_infoRight);

            _infoBanner.Children.Add(_infoMaster);

            _terminalsPanel.Children.Add(_console);
            
            Gui.AddToViewport(_infoBanner);
            Gui.AddToViewport(_terminalsPanel);
            Gui.AddToViewport(_sidePanel);
            Gui.AddToViewport(_displaysPanel);
        }

        private void StyleGui()
        {
            // Set viewport anchors for the desktop UIs.
            var h = Gui.GetScaledHeight(ViewportBounds.Height);
            _infoBanner.ViewportAnchor = new FreePanel.CanvasAnchor(0, 0, 1, 0);
            _terminalsPanel.ViewportAnchor = new FreePanel.CanvasAnchor(0.17f, 0.7f, 0.83f, 0.3f);
            _sidePanel.ViewportAnchor = new FreePanel.CanvasAnchor(0, 28f / h, 0.17f, 1 - (28f / h));
            _displaysPanel.ViewportAnchor = new FreePanel.CanvasAnchor(0.17f, 28f / h, 0.83f, 0.7f - (28f / h));
            
            // Fixed height for the status panell.
            _infoBanner.FixedHeight = 28;
            
            _sidePanel.BackColor = Color.Green;
            _terminalsPanel.BackColor = Color.Transparent;
            _displaysPanel.BackColor = Color.Red;
            
            _notificationBanner.ViewportAnchor = new FreePanel.CanvasAnchor(0.5f, 0, 0, 0);
            _notificationBanner.ViewportAlignment = new Vector2(0.5f, 0);
            _notificationBanner.FixedWidth = 460;
            
            _noteIcon.ImageMode = ImageMode.Rounded;
            _noteIcon.FixedHeight = 24;
            _noteIcon.FixedWidth = 24;
            _noteIcon.VerticalAlignment = VerticalAlignment.Top;

            _noteTitle.Properties.SetValue(FontStyle.Heading3);
            _noteTitle.ForeColor = Color.Cyan;

            _noteStacker.Direction = StackDirection.Horizontal;
            _noteButtonWrapper.Orientation = StackDirection.Horizontal;

            _noteStacker.Padding = 10;
            _noteIcon.Padding = 2;
            _noteInfoStacker.Padding = 2;
            _noteButtonWrapper.Padding = new Padding(0, 4, 0, 0);
            
            _settings.Text = "Settings";
            
            _playerAvatar.VerticalAlignment = VerticalAlignment.Center;
            _playerInfoStacker.VerticalAlignment = VerticalAlignment.Center;
            _settings.VerticalAlignment = VerticalAlignment.Center;

            _settings.Padding = new Padding(4, 0, 4, 0);
            
            _playerAvatar.FixedWidth = 24;
            _playerAvatar.FixedHeight = 24;
            _playerAvatar.ImageMode = ImageMode.Rounded;

            _infoMaster.Direction = StackDirection.Horizontal;
            _infoProfileCard.Direction = StackDirection.Horizontal;
            _infoRight.Direction = StackDirection.Horizontal;
            _infoLeft.Direction = StackDirection.Horizontal;

            _infoRight.HorizontalAlignment = HorizontalAlignment.Right;
            
            // Fills.
            _console.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _infoRight.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _infoLeft.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _playerInfoStacker.Padding = new Padding(3, 0, 0, 0);
            _infoMaster.Padding = new Padding(4, 2, 4, 2);

            _console.DrawBackgroundImage = false;
        }

        private void TrySave(IConsole console)
        {
            _saveManager.Save();
            _console.WriteLine($"&b * save successful * &B");
        }
        
        
    }
}