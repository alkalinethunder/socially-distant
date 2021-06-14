using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pango;
using RedTeam.Connectivity;
using RedTeam.Core.Components;
using RedTeam.Core.Config;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Gui.Elements;
using RedTeam.Core.SaveData;
using Thundershock;
using Thundershock.Components;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Input;
using Thundershock.Rendering;
using Color = Microsoft.Xna.Framework.Color;

namespace RedTeam
{
    public class MainMenu : Scene
    {
        public enum MenuState
        {
            MainMenu,
            Extensions,
            Play
        }

        #region Global Components
        
        private AnnouncementManager _announcements;
        private ContentManager _contentManager;
        private SaveManager _saveManager;

        #endregion
        
        #region Textures

        private Texture2D _mainIcon;
        
        #endregion

        #region Scene Components

        private SettingsComponent _settingsComponent;
        private Backdrop _backdrop;
        private GuiSystem _gui;
        private WindowManager _wm;
        private Backdrop _packBackdrop;
        
        #endregion
        
        #region UI

        private Stacker _careerErrorStacker = new();
        private TextBlock _careerErrorTitle = new();
        private TextBlock _careerErrorMessage = new();
        private Panel _backdropOverlay = new();
        private Stacker _menuArea = new();
        private Stacker _menuStacker = new();
        private Picture _logo = new();
        private Stacker _extensionsList = new();
        private Stacker _playStacker = new();
        private ScrollPanel _menuScroller = new();
        private IconButton _load = new();
        private IconButton _new = new();
        private Button _extensions = new();
        private Button _settings = new();
        private IconButton _continue = new();
        private Button _content = new();
        private Button _exit = new();
        private Button _back = new();
        private TextBlock _menuTitle = new();
        private Stacker _packInfoStacker = new();
        private Picture _packIcon = new();
        private Stacker _packTextStacker = new();
        private TextBlock _packTitle = new();
        private TextBlock _packAuthor = new();
        private Stacker _mainMenuStacker = new();
        
        #endregion
        
        #region State

        private InstalledContentPack _pack;
        private MenuState _state;
        private bool _hasShownAnnouncement = false;
        private SaveSlot[] _saves;
        private bool _isAnyCorrupted;

        #endregion
        
        protected override void OnLoad()
        {
            // Camera setup.
            Camera = new Camera2D();

            // Retrieve app component references.
            _saveManager = App.GetComponent<SaveManager>();
            _announcements = App.GetComponent<AnnouncementManager>();
            _contentManager = App.GetComponent<ContentManager>();

            // Add scene components.
            _backdrop = AddComponent<Backdrop>();
            _packBackdrop = AddComponent<Backdrop>();
            _gui = AddComponent<GuiSystem>();
            _wm = AddComponent<WindowManager>();

            // Set the backdrop wallpaper.
            _backdrop.Texture = App.Content.Load<Texture2D>("Backgrounds/DesktopBackgroundImage2");
            
            // Add root UI elements.
            _gui.AddToViewport(_logo);
            _gui.AddToViewport(_backdropOverlay);

            // Add the window manager layer.
            _wm.AddToGuiRoot(_gui);
            
            // Set up the layout of the game's logo.
            // It sits near the top of the screen in the middle.
            _logo.Image = App.Content.Load<Texture2D>("Textures/RedTeamLogo/redteam_banner_128x");
            _logo.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _logo.Properties.SetValue(FreePanel.AnchorProperty, FreePanel.CanvasAnchor.TopSide);
            _logo.HorizontalAlignment = HorizontalAlignment.Center;
            _logo.Margin = 60;
            
            // Set up the menu area. It goes exactly in the center of the screen.
            _backdropOverlay.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _backdropOverlay.Properties.SetValue(FreePanel.AnchorProperty, FreePanel.CanvasAnchor.Center);
            _backdropOverlay.Properties.SetValue(FreePanel.AlignmentProperty, new Vector2(0.5f, 0.5f));
            _menuArea.Padding = 15;
            
            // The menu area goes in the backdrop overlay.
            _backdropOverlay.Children.Add(_menuArea);
            
            // Set up the menu title.
            _menuTitle.TextAlign = TextAlign.Center;
            _menuTitle.Color = Color.Cyan;
            _menuTitle.Font = App.Content.Load<SpriteFont>("Fonts/MenuTitle");
            _menuArea.Children.Add(_menuTitle);
            
            // Set up the pack info area.
            _menuArea.Children.Add(_packInfoStacker);
            
            // The menu scroller is simple.
            _menuArea.Children.Add(_menuScroller);
            
            // Set up the back button.
            _back.Text = "Back";
            _back.HorizontalAlignment = HorizontalAlignment.Center;
            _menuArea.Children.Add(_back);
            
            // Set up the career error message.
            // This only shows in modder's mode, where there is no career.
            _careerErrorMessage.Text =
                "This version of RED TEAM does not have a Career mode. Career mode is only available in official releases of the game." +
                Environment.NewLine + Environment.NewLine +
                "You are free to play Custom Stories or make your own in this build.";
            _careerErrorTitle.Text = "* no career mode *";
            _careerErrorTitle.Color = Color.Red;
            _careerErrorMessage.Color = Color.White;
            _careerErrorTitle.TextAlign = TextAlign.Center;
            _careerErrorMessage.TextAlign = TextAlign.Center;
            _careerErrorStacker.Children.Add(_careerErrorTitle);
            _careerErrorStacker.Children.Add(_careerErrorMessage);
            _careerErrorTitle.Font = App.Content.Load<SpriteFont>("Fonts/ButtonDescription");
            _careerErrorMessage.Font = App.Content.Load<SpriteFont>("Fonts/Console/Regular");
            
            // Set up the Play menu.
            _playStacker.Children.Add(_continue);
            _playStacker.Children.Add(_new);
            _playStacker.Children.Add(_load);
            _playStacker.Direction = StackDirection.Horizontal;
            _playStacker.HorizontalAlignment = HorizontalAlignment.Center;
            
            // Set up the new, load and continue buttons.
            _new.Text = "New OS";
            _continue.Text = "Boot Last OS";
            _load.Text = "Other OS";

            // Set up the secondary menu. This is where you find extensions, settings, etc.
            _menuStacker.Children.Add(_extensions);
            _menuStacker.Children.Add(_content);
            _menuStacker.Children.Add(_settings);
            _menuStacker.Children.Add(_exit);
            _menuStacker.HorizontalAlignment = HorizontalAlignment.Center;
            _menuStacker.Direction = StackDirection.Horizontal;
            
            // Set up the secondary menu buttons.
            _exit.Text = "Shut Down";
            _settings.Text = "Options";
            _content.Text = "Content Manager";
            _extensions.Text = "More Stories";
            
            // Bind events.
            _new.MouseUp += NewAdvancedButtonOnMouseUp;
            _extensions.MouseUp += ExtensionsAdvancedButtonOnMouseUp;
            _exit.MouseUp += ExitAdvancedButtonOnMouseUp;
            _content.MouseUp += ContentOnMouseUp;
            _settings.MouseUp += SettingsOnMouseUp;
            _back.MouseUp += BackOnMouseUp;
            _continue.MouseUp += ContinueOnMouseUp;
            
            // Update the UI state.
            UpdateMenuScroller();

            // Done
            base.OnLoad();
        }

        private void ContinueOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                _saveManager.LoadGame(_saves.First());
                App.LoadScene<BootScreen>();
            }
        }

        private void SettingsOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                OpenSettings();
            }
        }

        private void ContentOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                OpenContentManager();
            }
        }

        private void NewAdvancedButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary && _pack != null) 
            {
                App.GetComponent<SaveManager>().NewGame(_pack);
                App.LoadScene<BootScreen>();
            }
        }

        private void SetupCareerSaves()
        {
            _pack = _contentManager.CareerPack;
            _isAnyCorrupted = _saveManager.SaveDatabase.HasAnyCorruptData;
            _saves = _saveManager.SaveDatabase.CareerSlots.ToArray();

            UpdatePlayMenu();
        }

        private void UpdatePlayMenu()
        {
            _continue.Visibility = _saves.Any() ? Visibility.Visible : Visibility.Collapsed;
            _load.Visibility = _saves.Length > 1 ? Visibility.Visible : Visibility.Collapsed;

        }
        
        private void UpdateMenuScroller()
        {
            _menuScroller.Children.Clear();
            _isAnyCorrupted = false;
            _saves = null;
            _mainMenuStacker.Children.Clear();
            
            switch (_state)
            {
                case MenuState.MainMenu:
                    _menuTitle.Text = "Main menu";
                    _menuScroller.Children.Add(_mainMenuStacker);
                    
                    if (_contentManager.HasCareerMode)
                    {
                        SetupCareerSaves();
                        _mainMenuStacker.Children.Add(_playStacker);
                    }
                    else
                    {
                        _mainMenuStacker.Children.Add(_careerErrorStacker);
                    }

                    _mainMenuStacker.Children.Add(_menuStacker);
                    _menuTitle.Visibility = Visibility.Visible;
                    _packInfoStacker.Visibility = Visibility.Collapsed;
                    break;
                case MenuState.Extensions:
                    _menuTitle.Text = "Extensions";
                    _menuScroller.Children.Add(_extensionsList);
                    _menuTitle.Visibility = Visibility.Visible;
                    _packInfoStacker.Visibility = Visibility.Collapsed;
                    break;
                case MenuState.Play:
                    _menuScroller.Children.Add(_playStacker);

                    if (_pack != null)
                    {
                        _menuTitle.Visibility = Visibility.Collapsed;
                        _packInfoStacker.Visibility = Visibility.Visible;

                        _packIcon.Image = _pack.Icon ?? _mainIcon;
                        _packTitle.Text = _pack.Name;
                        _packAuthor.Text = _pack.Author;
                    }
                    else
                    {
                        _packInfoStacker.Visibility = Visibility.Collapsed;
                        _menuTitle.Visibility = Visibility.Visible;
                        _menuTitle.Text = "Career";
                    }

                    if (_isAnyCorrupted)
                    {
                        _wm.ShowMessage("Corruption Detected",
                            "RED TEAM was unable to load some of the save games for " + _pack.Name +
                            ". You may wish to investigate this in the Content Manager. In some cases, corrupt saves can be recovered.", OpenContentManager);
                        
                        _isAnyCorrupted = false;
                    }
                    
                    break;
            }
        }
        
        private void BackOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            _pack = null;
            _state = MenuState.MainMenu;
            UpdateMenuScroller();
        }

        private void ExtensionsAdvancedButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            ListExtensions();
            _state = MenuState.Extensions;
            UpdateMenuScroller();
        }
        
        private void ExitAdvancedButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            App.Exit();
        }

        private void ListExtensions()
        {
            _extensionsList.Children.Clear();

            var packs = _contentManager.InstalledPacks.ToArray();

            if (packs.Any())
            {
                foreach (var pack in packs)
                {
                    var btn = new DetailedAdvancedButton();

                    btn.Text = pack.Author;
                    btn.Title = pack.Name;
                    btn.Icon = pack.Icon;
                    
                    btn.MouseUp += (o, a) =>
                    {
                        _pack = pack;
                        _state = MenuState.Play;
                        UpdateMenuScroller();
                    };
                    
                    _extensionsList.Children.Add(btn);
                }
            }
            else
            {
                var text = new TextBlock();
                text.Text = "There are no Content Packs to show here.";
                text.WrapMode = TextWrapMode.WordWrap;
                text.Color = Color.Cyan;
                _extensionsList.Children.Add(text);
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_announcements.IsReady && !_hasShownAnnouncement)
            {
                if (App.GetComponent<RedConfigManager>().ActiveConfig.ShowWhatsNew)
                {
                    ShowAnnouncement(_announcements.Announcement);
                }
                else
                {
                    _hasShownAnnouncement = true;
                }
            }
            
            switch (_state)
            {
                case MenuState.MainMenu:
                    _extensionsList.Visibility = Visibility.Collapsed;
                    _menuStacker.Visibility = Visibility.Visible;
                    _back.Visibility = Visibility.Collapsed;
                    break;
                case MenuState.Extensions:
                    _extensionsList.Visibility = Visibility.Visible;
                    _menuStacker.Visibility = Visibility.Collapsed;
                    _back.Visibility = Visibility.Visible;
                    break;
                case MenuState.Play:
                    _extensionsList.Visibility = Visibility.Collapsed;
                    _menuStacker.Visibility = Visibility.Collapsed;
                    _back.Visibility = Visibility.Visible;
                    break;
            }

            if (_pack != null)
            {
                _packBackdrop.Texture = _pack.Backdrop;
                _backdropOverlay.BackColor = Color.Black * 0.75f;
            }
            else
            {
                _packBackdrop.Texture = null;
                _backdropOverlay.BackColor = Color.Transparent;
            }
        }

        private void ShowAnnouncement(Announcement announcement)
        {
            var pane = _wm.CreateFloatingPane("Announcement");

            var panel = new Panel();
            panel.BackColor = ThundershockPlatform.HtmlColor("#222222");
            var stacker = new Stacker();
            stacker.Padding = 15;
            stacker.Margin = 15;
            var title = new TextBlock();
            title.Text = announcement.Title;
            title.Color = Color.Cyan;
            title.Font = _menuTitle.Font;
            stacker.Children.Add(title);
            var excerpt = new TextBlock();
            excerpt.Color = Color.White;
            excerpt.Text = announcement.Excerpt;
            stacker.Children.Add(excerpt);

            var doNotShowAgain = new CheckBox();
            var doNotShowLabel = new TextBlock();
            doNotShowLabel.Text = "Don't show what's new on startup";
            doNotShowLabel.Color = Color.White;
            doNotShowAgain.Children.Add(doNotShowLabel);

            var readMoreLink = new TextBlock();
            readMoreLink.Color = Color.Cyan;
            readMoreLink.Text = "Read More";
            readMoreLink.IsInteractable = true;
            readMoreLink.MouseDown += (o, a) =>
            {
                if (a.Button == MouseButton.Primary)
                {
                    ThundershockPlatform.OpenFile(announcement.Link);
                }
            };
            stacker.Children.Add(readMoreLink);
            
            var buttonRow = new Stacker();
            buttonRow.Direction = StackDirection.Horizontal;
            buttonRow.Children.Add(doNotShowAgain);
            doNotShowAgain.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            var doneButton = new Button();
            doneButton.Text = "Close";
            doneButton.MouseDown += (o, a) =>
            {
                if (doNotShowAgain.IsChecked)
                {
                    App.GetComponent<RedConfigManager>().ActiveConfig.ShowWhatsNew = false;
                    App.GetComponent<RedConfigManager>().ApplyChanges();
                    _wm.ShowMessage("Settings Changed",
                        "You've chosen to hide the What's New screen on startup. You can change this preference in System Settings.");
                }
                
                pane.Parent.Children.Remove(pane);
            };
            buttonRow.Children.Add(doneButton);

            doneButton.VerticalAlignment = VerticalAlignment.Center;
            doNotShowAgain.VerticalAlignment = VerticalAlignment.Center;
            
            stacker.Children.Add(buttonRow);
            
            panel.Children.Add(stacker);
            pane.Content.Add(panel);
            
            pane.BorderColor = ThundershockPlatform.HtmlColor("#f71b1b");

            _hasShownAnnouncement = true;
        }

        private void OpenContentManager()
        {
            
        }

        private void OpenSettings()
        {
            if (!HasComponent<SettingsComponent>())
                AddComponent<SettingsComponent>();
        }
    }
}