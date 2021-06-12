using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Core.Components;
using RedTeam.Core.Config;
using RedTeam.Core.Gui.Elements;
using Thundershock;
using Thundershock.Config;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Input;

namespace RedTeam
{
    public class SettingsComponent : SceneComponent
    {
        private ConfigurationManager _config;
        private RedConfigManager _redConfig;
        
        private BasicSettingsCategory _currentCategory;
        private string _modCategoryName = string.Empty;
        
        private Pane _settingsPane;
        private WindowManager _wm;
        private Panel _bg = new();
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
        private LabeledDropdown _primaryMouse = new();
        private LabeledDropdown _consolePalette = new();

        // About UI
        private Stacker _aboutStacker = new();
        
        
        protected override void OnLoad()
        {
            _config = App.GetComponent<ConfigurationManager>();
            _redConfig = App.GetComponent<RedConfigManager>();

            _wm = Scene.GetComponent<WindowManager>();

            _settingsPane = _wm.CreateFloatingPane("System Settings");

            _bg.BackColor = ThundershockPlatform.HtmlColor("#222222");

            // General layout tree.
            _sidebarScroller.Children.Add(_sidebar);
            _splitter.Children.Add(_sidebarScroller);
            _splitter.Children.Add(_mainScroller);
            _masterStacker.Children.Add(_splitter);
            _masterStacker.Children.Add(_buttonList);
            _bg.Children.Add(_masterStacker);
            _settingsPane.Content.Add(_bg);
            
            // Button list and splitter are both horizontal stackers.
            _splitter.Direction = StackDirection.Horizontal;
            _buttonList.Direction = StackDirection.Horizontal;
            
            // The splitter and main settings area both fill available space.
            _splitter.Properties.SetValue(Stacker.FillProperty, true);
            _mainScroller.Properties.SetValue(Stacker.FillProperty, true);

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
            _restoreDefaults.Properties.SetValue(Stacker.FillProperty, true);

            // Set up settings categories.
            UpdateCategories();
            
            // Build the Graphics UI.
            BuildGraphicsUI();
            
            base.OnLoad();
        }

        private void BuildGraphicsUI()
        {
            var displayHeader = new TextBlock();
            var ppHeader = new TextBlock();
            var guiHeader = new TextBlock();

            displayHeader.Text = "Display";
            ppHeader.Text = "Effects";
            guiHeader.Text = "User Interface";

            displayHeader.Font = App.Content.Load<SpriteFont>("Fonts/MenuTitle");
            ppHeader.Font = App.Content.Load<SpriteFont>("Fonts/MenuTitle");
            guiHeader.Font = App.Content.Load<SpriteFont>("Fonts/MenuTitle");

            displayHeader.Color = Color.Cyan;
            ppHeader.Color = Color.Cyan;
            guiHeader.Color = Color.Cyan;

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
            foreach (var displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
                .OrderByDescending(x => x.Width * x.Height).Select(x => $"{x.Width}x{x.Height}").Distinct())
            {
                _resolution.AddItem(displayMode);
                if (displayMode == $"{_config.GetDisplayMode().Width}x{_config.GetDisplayMode().Height}")
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

        private void ConsoleFontSizeOnSelectedIndexChanged(Object sender, EventArgs e)
        {
            _redConfig.ActiveConfig.ConsoleFontSize = _consoleFontSize.SelectedIndex;
            _redConfig.ApplyChanges();
        }

        private void VsyncOnCheckStateChanged(object? sender, EventArgs e)
        {
            _config.ActiveConfig.VSync = _vsync.IsChecked;
            _config.ApplyChanges();
        }

        private void ShadowmaskOnCheckStateChanged(object? sender, EventArgs e)
        {
            _config.ActiveConfig.Effects.ShadowMask = _shadowmask.IsChecked;
            _config.ApplyChanges();
        }

        private void BloomOnCheckStateChanged(object? sender, EventArgs e)
        {
            _config.ActiveConfig.Effects.Bloom = _bloom.IsChecked;
            _config.ApplyChanges();
        }

        private void WindowModeOnCheckStateChanged(object? sender, EventArgs e)
        {
            _config.ActiveConfig.IsFullscreen = _windowMode.IsChecked;
            _config.ApplyChanges();
        }

        private void ResolutionOnSelectedIndexChanged(object? sender, EventArgs e)
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
            basicHeader.Text = "Settings";
            basicHeader.Font = App.Content.Load<SpriteFont>("Fonts/ButtonDescription");
            basicHeader.Color = Color.Cyan;
            _sidebar.Children.Add(basicHeader);

            basicHeader.Padding = new Padding(0, 0, 0, 7);
            
            foreach (var setting in Enum.GetNames(typeof(BasicSettingsCategory)))
            {
                if (setting == nameof(BasicSettingsCategory.ModSettings))
                    continue;

                var btn = new Button();
                btn.Text = setting;
                btn.MouseUp += (o, a) =>
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