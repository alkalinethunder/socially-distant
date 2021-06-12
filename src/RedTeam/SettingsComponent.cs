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
        
        // Audio Settings UI
        private Stacker _audioStacker = new();
        
        // Gameplay Settings UI
        private Stacker _gameplayStacker = new();
        
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
            _masterStacker.Padding = 15;
            
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
            var resolutionSelector = new LabeledDropdown();
            var fullscreenMode = new LabeledDropdown();

            fullscreenMode.Title = "Window mode";
            resolutionSelector.Title = "Screen Resolution";

            var windowStacker = new Stacker();
            windowStacker.Direction = StackDirection.Horizontal;

            windowStacker.Children.Add(resolutionSelector);
            windowStacker.Children.Add(fullscreenMode);

            resolutionSelector.Padding = 30;
            fullscreenMode.Padding = 30;
            
            _graphicsStacker.Children.Add(windowStacker);

            var i = 0;
            foreach (var displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
                .OrderByDescending(x => x.Width * x.Height).Select(x => $"{x.Width}x{x.Height}").Distinct())
            {
                resolutionSelector.AddItem(displayMode);
                if (displayMode == $"{_config.GetDisplayMode().Width}x{_config.GetDisplayMode().Height}")
                {
                    resolutionSelector.SelectedIndex = i;
                }
                i++;
            }
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