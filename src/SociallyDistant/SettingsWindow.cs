using System;
using System.Linq;
using SociallyDistant.Core;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.Gui.Elements;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Config;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant
{
    public class SettingsWindow : Window
    {
        private ConfigurationManager _config;
        private RedConfigManager _redConfig;
        
        private BasicSettingsCategory _currentCategory;

        private Stacker _masterStacker = new();
        private Stacker _buttonList = new();
        private Stacker _splitter = new();
        private ScrollPanel _sidebarScroller = new();
        private ScrollPanel _mainScroller = new();
        private Stacker _sidebar = new();
        
        // Global buttons.
        private Button _close = new();
        private Button _restoreDefaults = new();

        // Graphics Settings UI
        private Stacker _graphicsStacker = new();
        private LabeledDropdown _resolution = new();
        private SettingToggle _windowMode = new();
        private LabeledDropdown _consoleFontSize = new();
        private LabeledDropdown _guiScale = new();
        private SettingToggle _bloom = new();
        private SettingToggle _shadowmask = new();
        private SettingToggle _glitches = new();
        private SettingToggle _vsync = new();

        // Audio Settings UI
        private Stacker _audioStacker = new();
        
        // Gameplay Settings UI
        private Stacker _gameplayStacker = new();

        // About UI
        private Stacker _aboutStacker = new();
        private Picture _redteamLogo = new();
        private TextBlock _redteamDescription = new();
        private Button _openLocalData = new();
        private Button _openWebsite = new();
        private Button _github = new();
        private CheckBox _enableWhatsNew = new();
        private TextBlock _whatsNewLabel = new();
        
        protected override void OnOpened()
        {
            _config = Scene.Game.GetComponent<ConfigurationManager>();
            _redConfig = Scene.Game.GetComponent<RedConfigManager>();
            
            FixedWidth = 720;
            FixedHeight = 500;
            
            // General layout tree.
            _sidebarScroller.Children.Add(_sidebar);
            _splitter.Children.Add(_sidebarScroller);
            _splitter.Children.Add(_mainScroller);
            _masterStacker.Children.Add(_splitter);
            _masterStacker.Children.Add(_buttonList);
            
            Children.Add(_masterStacker);

            // Button list and splitter are both horizontal stackers.
            _splitter.Direction = StackDirection.Horizontal;
            _buttonList.Direction = StackDirection.Horizontal;
            
            // The splitter and main settings area both fill available space.
            _splitter.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _mainScroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            // Padding
            _masterStacker.Padding = 10;
            _sidebarScroller.Padding = 5;
            _mainScroller.Padding = 5;
            _buttonList.Padding = 5;
            
            // Set up the button list.
            _buttonList.Children.Add(_restoreDefaults);
            _buttonList.Children.Add(_close);
            
            // Set up the buttons themselves.
            _close.Text = "Close";
            _restoreDefaults.Text = "Restore Default Settings";
            
            // Make the restore button fill available space but align it to the left.
            _restoreDefaults.HorizontalAlignment = HorizontalAlignment.Left;
            _restoreDefaults.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            // Set up settings categories.
            UpdateCategories();
            
            // Build the Graphics UI.
            BuildGraphicsUi();
            
            // Build the about screen.
            BuildAboutScreen();
            
            _close.MouseUp += CloseOnMouseUp;
        }
        
        private void CloseOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                Close();
            }
        }

        private void BuildGraphicsUi()
        {
            var displayHeader = new TextBlock();
            var ppHeader = new TextBlock();
            var guiHeader = new TextBlock();

            displayHeader.Text = "Display";
            ppHeader.Text = "Effects";
            guiHeader.Text = "User Interface";
            
            displayHeader.ForeColor = Color.Cyan;
            displayHeader.Properties.SetValue(FontStyle.Heading2);
            ppHeader.ForeColor = Color.Cyan;
            ppHeader.Properties.SetValue(FontStyle.Heading2);
            guiHeader.ForeColor = Color.Cyan;
            guiHeader.Properties.SetValue(FontStyle.Heading2);

            _resolution.Title = "Screen Resolution";
            _windowMode.Title = "Fullscreen";
            _bloom.Title = "Bloom";
            _shadowmask.Title = "CRT Shadow-mask";
            _glitches.Title = "Glitch Effects";
            _vsync.Title = "Vertical Sync";
            _guiScale.Title = "GUI Scale";
            _consoleFontSize.Title = "Console Font Size";
            
            _bloom.ToolTip = "Bloom effect makes bright text and objects glow.";
            _shadowmask.ToolTip =
                "Overlay the screen with a color CRT shadow-mask to increase the 80s retro computer aesthetic.";
            _glitches.ToolTip = "Screw with the screen when glitch sounds play.";
            
            var windowStacker = new WrapPanel();
            var postProcessStacker = new WrapPanel();
            var guiStacker = new WrapPanel();
            
            windowStacker.Orientation = StackDirection.Horizontal;
            postProcessStacker.Orientation = StackDirection.Horizontal;
            guiStacker.Orientation = StackDirection.Horizontal;
            
            windowStacker.Children.Add(_resolution);
            windowStacker.Children.Add(_windowMode);
            windowStacker.Children.Add(_vsync);
            
            postProcessStacker.Children.Add(_bloom);
            postProcessStacker.Children.Add(_shadowmask);
            postProcessStacker.Children.Add(_glitches);

            guiStacker.Children.Add(_consoleFontSize);
            guiStacker.Children.Add(_guiScale);
            
            _resolution.Padding = 15;
            _windowMode.Padding = 15;
            _vsync.Padding = 15;
            _bloom.Padding = 15;
            _shadowmask.Padding = 15;
            _glitches.Padding = 15;
            _consoleFontSize.Padding = 15;
            _guiScale.Padding = 15;
            
            _graphicsStacker.Children.Add(displayHeader);
            _graphicsStacker.Children.Add(windowStacker);
            _graphicsStacker.Children.Add(ppHeader);
            _graphicsStacker.Children.Add(postProcessStacker);
            _graphicsStacker.Children.Add(guiHeader);
            _graphicsStacker.Children.Add(guiStacker);
            
            _windowMode.IsChecked = _config.ActiveConfig.IsFullscreen;
            _vsync.IsChecked = _config.ActiveConfig.VSync;
            _bloom.IsChecked = _config.ActiveConfig.Effects.Bloom;
            _shadowmask.IsChecked = _config.ActiveConfig.Effects.ShadowMask;

            var i = 0;
            var current = $"{_config.GetDisplayMode().Width}x{_config.GetDisplayMode().Height}";
            foreach (var displayMode in _config.GetAvailableDisplayModes()
                .Select(x=>$"{x.Width}x{x.Height}"))
            {
                _resolution.AddItem(displayMode);
                if (displayMode == current)
                {
                    _resolution.SelectedIndex = i;
                }
                i++;
            }
            
            // Console Font Scale
            _consoleFontSize.AddItem("Normal");
            _consoleFontSize.AddItem("Medium");
            _consoleFontSize.AddItem("Large");
            _consoleFontSize.SelectedIndex = _redConfig.ActiveConfig.ConsoleFontSize;
            
            // Bind resolution change event.
            _resolution.SelectedIndexChanged += ResolutionOnSelectedIndexChanged;
            _windowMode.CheckStateChanged += WindowModeOnCheckStateChanged;
            _bloom.CheckStateChanged += BloomOnCheckStateChanged;
            _shadowmask.CheckStateChanged += ShadowmaskOnCheckStateChanged;
            _vsync.CheckStateChanged += VsyncOnCheckStateChanged;
            _consoleFontSize.SelectedIndexChanged += ConsoleFontSizeOnSelectedIndexChanged;
        }

        private void BuildAboutScreen()
        {
            // Text
            _redteamDescription.Text =
                "Red Team is a semi-realistic hacking game. Developed by Michael VanOverbeek. Powered by the Thundershock Engine. Logo design courtesy of Logan Lowe."
                + Environment.NewLine
                + Environment.NewLine
                + "This game uses open-source software. For more information, see Credits.";
            _openLocalData.Text = "Open User Data Folder";
            _openWebsite.Text = "Open Website";
            _github.Text = "Source Code";
            _whatsNewLabel.Text = "Show the What's New screen on startup";
            
            // Alignments.
            _redteamLogo.HorizontalAlignment = HorizontalAlignment.Center;
            _openLocalData.HorizontalAlignment = HorizontalAlignment.Center;
            _openWebsite.HorizontalAlignment = HorizontalAlignment.Center;
            _github.HorizontalAlignment = HorizontalAlignment.Center;
            _enableWhatsNew.HorizontalAlignment = HorizontalAlignment.Center;
            
            // Text alignment
            _redteamDescription.TextAlign = TextAlign.Center;
            
            // Padding
            _redteamLogo.Padding = 5;
            _redteamDescription.Padding = new Padding(5, 0, 5, 10);
            _openLocalData.Padding = 2;
            _openWebsite.Padding = 2;
            _github.Padding = 2;
            _enableWhatsNew.Padding = 6;
            
            // GUI tree.
            _enableWhatsNew.Children.Add(_whatsNewLabel);
            _aboutStacker.Children.Add(_redteamLogo);
            _aboutStacker.Children.Add(_redteamDescription);
            _aboutStacker.Children.Add(_openLocalData);
            _aboutStacker.Children.Add(_openWebsite);
            _aboutStacker.Children.Add(_github);
            _aboutStacker.Children.Add(_enableWhatsNew);

            _openLocalData.MouseUp += (_, a) =>
            {
                if (a.Button == MouseButton.Primary)
                {
                    ThundershockPlatform.OpenFile(ThundershockPlatform.LocalDataPath);
                }
            };
            
            _openWebsite.MouseUp += (_, a) =>
            {
                if (a.Button == MouseButton.Primary)
                {
                    ThundershockPlatform.OpenFile("https://aklnthndr.dev/");
                }
            };
            
            _github.MouseUp += (_, a) =>
            {
                if (a.Button == MouseButton.Primary)
                {
                    ThundershockPlatform.OpenFile("https://github.com/redteam-os");
                }
            };

            _enableWhatsNew.IsChecked = _redConfig.ActiveConfig.ShowWhatsNew;
            
            _enableWhatsNew.CheckStateChanged += (_, _) =>
            {
                _redConfig.ActiveConfig.ShowWhatsNew = _enableWhatsNew.IsChecked;
                _redConfig.ApplyChanges();
            };
        }
        
        private void ConsoleFontSizeOnSelectedIndexChanged(Object sender, EventArgs e)
        {
            _redConfig.ActiveConfig.ConsoleFontSize = _consoleFontSize.SelectedIndex;
            _redConfig.ApplyChanges();
        }

        private void VsyncOnCheckStateChanged(object sender, EventArgs e)
        {
            _config.ActiveConfig.VSync = _vsync.IsChecked;
            _config.ApplyChanges();
        }

        private void ShadowmaskOnCheckStateChanged(object sender, EventArgs e)
        {
            _config.ActiveConfig.Effects.ShadowMask = _shadowmask.IsChecked;
            _config.ApplyChanges();
        }

        private void BloomOnCheckStateChanged(object sender, EventArgs e)
        {
            _config.ActiveConfig.Effects.Bloom = _bloom.IsChecked;
            _config.ApplyChanges();
        }

        private void WindowModeOnCheckStateChanged(object sender, EventArgs e)
        {
            _config.ActiveConfig.IsFullscreen = _windowMode.IsChecked;
            _config.ApplyChanges();
        }

        private void ResolutionOnSelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the desired resolution.
            var res = _resolution.SelectedItem;
            
            // Give it to Thundershock!
            _config.SetDisplayMode(res);
            _config.ApplyChanges();
        }

        private void UpdateCategories()
        {
            _sidebar.Children.Clear();

            var basicHeader = new TextBlock();
            basicHeader.Properties.SetValue(FontStyle.Heading2);
            basicHeader.Text = "Settings";
            basicHeader.ForeColor = Color.Cyan;
            _sidebar.Children.Add(basicHeader);

            basicHeader.Padding = new Padding(0, 0, 0, 7);
            
            foreach (var setting in Enum.GetNames(typeof(BasicSettingsCategory)))
            {
                if (setting == nameof(BasicSettingsCategory.ModSettings))
                    continue;

                var btn = new Button();
                btn.Text = setting;
                btn.MouseUp += (_, a) =>
                {
                    if (a.Button == MouseButton.Primary)
                    {
                        _currentCategory = (BasicSettingsCategory) Enum.Parse(typeof(BasicSettingsCategory), setting);
                        UpdateCategories();
                    }
                };

                btn.IsActive = _currentCategory.ToString() == setting;

                _sidebar.Children.Add(btn);
            }

            _mainScroller.Children.Clear();

            switch (_currentCategory)
            {
                case BasicSettingsCategory.Graphics:
                    _mainScroller.Children.Add(_graphicsStacker);
                    break;
                case BasicSettingsCategory.About:
                    _mainScroller.Children.Add(_aboutStacker);
                    break;
                case BasicSettingsCategory.Audio:
                    _mainScroller.Children.Add(_audioStacker);
                    break;
                case BasicSettingsCategory.Gameplay:
                    _mainScroller.Children.Add(_gameplayStacker);
                    break;
            }
        }

        public enum BasicSettingsCategory
        {
            Graphics,
            Audio,
            Gameplay,
            About,
            ModSettings
        }   
    }
}