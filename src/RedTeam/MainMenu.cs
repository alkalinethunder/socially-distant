using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Connectivity;
using RedTeam.Core.Components;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Gui.Elements;
using RedTeam.Core.SaveData;
using Thundershock;
using Thundershock.Components;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Input;
using Thundershock.Rendering;

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

        private AnnouncementManager _announcements;
        private Texture2D _mainIcon;
        private Panel _backdropOverlay = new();
        private ContentManager _contentManager;
        private InstalledContentPack _pack;
        private Backdrop _backdrop;
        private Backdrop _packBackdrop;
        private GuiSystem _gui;
        private WindowManager _wm;
        private MenuState _state;
        private Stacker _menuArea = new();
        private Stacker _menuStacker = new();
        private Picture _logo = new();
        private Stacker _extensionsList = new();
        private Stacker _playStacker = new();
        private ScrollPanel _menuScroller = new();
        private DetailedAdvancedButton _careerAdvancedButton = new();
        private DetailedAdvancedButton _loadAdvancedButton = new();
        private DetailedAdvancedButton _newAdvancedButton = new();
        private DetailedAdvancedButton _extensionsAdvancedButton = new();
        private DetailedAdvancedButton _settingsAdvancedButton = new();
        private DetailedAdvancedButton _continueAdvancedButton = new();
        private DetailedAdvancedButton _contentManagerAdvancedButton = new();
        private DetailedAdvancedButton _exitAdvancedButton = new();
        private DetailedAdvancedButton _back = new();
        private TextBlock _menuTitle = new();
        private Stacker _packInfoStacker = new();
        private Picture _packIcon = new();
        private Stacker _packTextStacker = new();
        private TextBlock _packTitle = new();
        private TextBlock _packAuthor = new();
        private bool _hasShownAnnouncement = false;
        
        protected override void OnLoad()
        {
            Camera = new Camera2D();

            _announcements = App.GetComponent<AnnouncementManager>();
            
            _contentManager = App.GetComponent<ContentManager>();
            _backdrop = AddComponent<Backdrop>();
            _packBackdrop = AddComponent<Backdrop>();
            _gui = AddComponent<GuiSystem>();
            _wm = AddComponent<WindowManager>();

            _backdrop.Texture = App.Content.Load<Texture2D>("Backgrounds/DesktopBackgroundImage2");
            
            _gui.AddToViewport(_backdropOverlay);
            _gui.AddToViewport(_logo);
            _gui.AddToViewport(_menuArea);
            _gui.AddToViewport(_back);

            _menuArea.Children.Add(_menuTitle);
            _menuArea.Children.Add(_packInfoStacker);
            _menuArea.Children.Add(_menuScroller);
            
            _logo.Image = App.Content.Load<Texture2D>("Textures/RedTeamLogo/redteam_banner_128x");
            _logo.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _logo.Properties.SetValue(FreePanel.AnchorProperty, FreePanel.CanvasAnchor.TopLeft);
            _logo.Margin = 45;

            _logo.MaximumWidth = _logo.Image.Width;
            _mainIcon = _logo.Image;
            
            _menuArea.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _menuArea.Properties.SetValue(FreePanel.AlignmentProperty, new Vector2(0, 0.5f));
            _menuArea.Properties.SetValue(FreePanel.AnchorProperty, new FreePanel.CanvasAnchor(0, 0.5f, 0, 0));
            _menuArea.VerticalAlignment = VerticalAlignment.Center;
            _menuArea.MaximumHeight = 700;
            _menuArea.Margin = 45;
            
            _menuStacker.Children.Add(_careerAdvancedButton);
            _menuStacker.Children.Add(_extensionsAdvancedButton);
            _menuStacker.Children.Add(_settingsAdvancedButton);
            _menuStacker.Children.Add(_contentManagerAdvancedButton);
            _menuStacker.Children.Add(_exitAdvancedButton);
            
            _continueAdvancedButton.Title = "CONTINUE";
            _loadAdvancedButton.Title = "LOG IN";
            _newAdvancedButton.Title = "NEW VM";
            _careerAdvancedButton.Title = "CAREER";
            _extensionsAdvancedButton.Title = "EXTENSIONS";
            _contentManagerAdvancedButton.Title = "CONTENT MANAGER";
            _settingsAdvancedButton.Title = "SYSTEM SETTINGS";
            _exitAdvancedButton.Title = "SHUT DOWN";

            _loadAdvancedButton.Text = string.Empty;
            _newAdvancedButton.Text = string.Empty;
            _continueAdvancedButton.Text = "Last save name here";
            _careerAdvancedButton.Text = "Become a RED TEAM Agent.";
            _extensionsAdvancedButton.Text = "Launch an installed Content Pack.";
            _contentManagerAdvancedButton.Text = "View & manage currently available Content Packs and Mods.";
            _settingsAdvancedButton.Text = "";
            _exitAdvancedButton.Text = "";

            _menuStacker.MaximumWidth = _logo.Image.Width;

            _continueAdvancedButton.Enabled = false;
            
            base.OnLoad();
         
            _careerAdvancedButton.MouseUp += CareerAdvancedButtonOnMouseUp;
            _careerAdvancedButton.Enabled = _contentManager.HasCareerMode;
            _exitAdvancedButton.MouseUp += ExitAdvancedButtonOnMouseUp;
            _extensionsAdvancedButton.MouseUp += ExtensionsAdvancedButtonOnMouseUp;

            _backdropOverlay.BackColor = Color.Black;
            _backdropOverlay.FixedWidth = _logo.Image.Width;
            _backdropOverlay.Margin = 45;
            _backdropOverlay.HorizontalAlignment = HorizontalAlignment.Left;

            _back.Title = "Back";
            _back.Margin = 45;
            _back.VerticalAlignment = VerticalAlignment.Bottom;
            _back.HorizontalAlignment = HorizontalAlignment.Left;
            _back.Text = string.Empty;
            _back.MouseUp += BackOnMouseUp;

            _menuScroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _menuTitle.Color = Color.Cyan;
            _menuTitle.Font = App.Content.Load<SpriteFont>("Fonts/MenuTitle");

            _playStacker.Children.Add(_continueAdvancedButton);
            _playStacker.Children.Add(_newAdvancedButton);
            _playStacker.Children.Add(_loadAdvancedButton);
            
            UpdateMenuScroller();

            _packInfoStacker.Direction = StackDirection.Horizontal;

            _packInfoStacker.Children.Add(_packIcon);
            _packInfoStacker.Children.Add(_packTextStacker);
            _packTextStacker.Children.Add(_packTitle);
            _packTextStacker.Children.Add(_packAuthor);

            _packTitle.Font = _menuTitle.Font;
            _packTitle.Color = Color.Cyan;
            _packTitle.WrapMode = TextWrapMode.WordWrap;
            _packAuthor.Color = ThundershockPlatform.HtmlColor("#999999");
            _packAuthor.WrapMode = TextWrapMode.WordWrap;
            _packAuthor.Font = App.Content.Load<SpriteFont>("Fonts/ButtonDescription");

            _packIcon.Padding = 2;
            _packTextStacker.Padding = 2;
            _packInfoStacker.Margin = 3;
            _packIcon.FixedWidth = 64;
            _packIcon.FixedHeight = 64;
            _packIcon.VerticalAlignment = VerticalAlignment.Center;
            _packTextStacker.VerticalAlignment = VerticalAlignment.Center;

            _menuTitle.Padding = new Padding(0, 0, 0, 8);
            _packInfoStacker.Padding = new Padding(0, 0, 0, 8);
            
            _newAdvancedButton.MouseUp += NewAdvancedButtonOnMouseUp;

            _wm.AddToGuiRoot(_gui);
        }

        private void NewAdvancedButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary && _pack != null) 
            {
                App.GetComponent<SaveManager>().NewGame(_pack);
                App.LoadScene<RedTeamHackerScene>();
            }
        }

        private void UpdateMenuScroller()
        {
            _menuScroller.Children.Clear();

            switch (_state)
            {
                case MenuState.MainMenu:
                    _menuTitle.Text = "Main menu";
                    _menuScroller.Children.Add(_menuStacker);
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

        private void CareerAdvancedButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            _state = MenuState.Play;
            _pack = _contentManager.CareerPack;
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
                ShowAnnouncement(_announcements.Announcement);
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
                _backdropOverlay.Opacity = 0.75f;
            }
            else
            {
                _packBackdrop.Texture = null;
                _backdropOverlay.Opacity = 0;
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
    }
}