using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Core.Components;
using RedTeam.Core.Gui.Elements;
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
        private Backdrop _backdrop;
        private GuiSystem _gui;
        private WindowManager _wm;

        private Stacker _menuStacker = new();
        private Picture _logo = new();
        private DetailedButton _careerButton = new();
        private DetailedButton _extensionsButton = new();
        private DetailedButton _settingsButton = new();
        private DetailedButton _continueButton = new();
        private DetailedButton _contentManagerButton = new();
        private DetailedButton _exitButton = new();

        protected override void OnLoad()
        {
            Camera = new Camera2D();

            _backdrop = AddComponent<Backdrop>();
            _gui = AddComponent<GuiSystem>();
            _wm = AddComponent<WindowManager>();

            _backdrop.Texture = App.Content.Load<Texture2D>("Backgrounds/DesktopBackgroundImage2");
            
            _gui.AddToViewport(_logo);
            _gui.AddToViewport(_menuStacker);

            _logo.Image = App.Content.Load<Texture2D>("Textures/RedTeamLogo/redteam_banner_128x");
            _logo.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _logo.Properties.SetValue(FreePanel.AnchorProperty, FreePanel.CanvasAnchor.TopLeft);
            _logo.Margin = 45;
            
            _menuStacker.Properties.SetValue(FreePanel.AutoSizeProperty, true);
            _menuStacker.Properties.SetValue(FreePanel.AlignmentProperty, new Vector2(0, 0.5f));
            _menuStacker.Properties.SetValue(FreePanel.AnchorProperty, new FreePanel.CanvasAnchor(0, 0.5f, 0, 0));
            _menuStacker.VerticalAlignment = VerticalAlignment.Center;

            _menuStacker.Margin = 45;
            
            _menuStacker.Children.Add(_continueButton);
            _menuStacker.Children.Add(_careerButton);
            _menuStacker.Children.Add(_extensionsButton);
            _menuStacker.Children.Add(_settingsButton);
            _menuStacker.Children.Add(_contentManagerButton);
            _menuStacker.Children.Add(_exitButton);

            _continueButton.Title = "CONTINUE";
            _careerButton.Title = "CAREER";
            _extensionsButton.Title = "EXTENSIONS";
            _contentManagerButton.Title = "CONTENT MANAGER";
            _settingsButton.Title = "SYSTEM SETTINGS";
            _exitButton.Title = "SHUT DOWN";

            _continueButton.Text = "Last save name here";
            _careerButton.Text = "Become a RED TEAM Agent.";
            _extensionsButton.Text = "Launch an installed Content Pack.";
            _contentManagerButton.Text = "View & manage currently available Content Packs and Mods.";
            _settingsButton.Text = "";
            _exitButton.Text = "";

            _menuStacker.MaximumWidth = _logo.Image.Width;

            _continueButton.Enabled = false;
            
            base.OnLoad();
         
            _careerButton.MouseUp += CareerButtonOnMouseUp;
            _exitButton.MouseUp += ExitButtonOnMouseUp;
        }

        private void CareerButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            App.LoadScene<RedTeamHackerScene>();
        }

        private void ExitButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            App.Exit();
        }
    }
}