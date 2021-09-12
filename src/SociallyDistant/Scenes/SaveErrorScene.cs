using System;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Gui;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Scenes
{
    public class SaveErrorScene : Scene
    {
        #region UI Elements

        private Panel _mainPanel = new();
        private Stacker _mainStacker = new();
        private Picture _sociallyDistant = new();
        private TextBlock _title = new();
        private TextBlock _errorTitle = new();
        private TextBlock _errorMessage = new();
        private Stacker _buttonList = new();
        private Button _newGame = new();
        private Button _githubIssues = new();
        private ScrollPanel _exceptionMessageScroller = new();
        private TextBlock _exceptionText = new();
        
        #endregion
        
        #region Static variables

        private static Exception _exceptionToHandle;

        #endregion

        #region Static methods

        public static void SetException(Exception ex)
        {
            _exceptionToHandle = ex;
        }

        #endregion
        
        protected override void OnLoad()
        {
            if (_exceptionToHandle == null)
            {
                SetException(Game.GetComponent<SaveManager>().PreloadException);
            }

            _exceptionMessageScroller.Children.Add(_exceptionText);
            
            _buttonList.Children.Add(_githubIssues);
            _buttonList.Children.Add(_newGame);
            
            _mainStacker.Children.Add(_sociallyDistant);
            _mainStacker.Children.Add(_title);
            _mainStacker.Children.Add(_errorTitle);
            _mainStacker.Children.Add(_errorMessage);
            _mainStacker.Children.Add(_buttonList);
            _mainStacker.Children.Add(_exceptionMessageScroller);
            
            _mainPanel.Children.Add(_mainStacker);
            Gui.AddToViewport(_mainPanel);

            _title.Properties.SetValue(FontStyle.Heading1);
            _title.ForeColor = Color.FromHtml("#f71b1b");
            _errorTitle.Properties.SetValue(FontStyle.Heading3);
            _exceptionText.Properties.SetValue(FontStyle.Code);

            _title.Text = "Your game has crashed!";
            _errorTitle.Text = "An error occurred loading your profile.";
            _errorMessage.Text =
                "Unfortunately, the game could not load your Profile. This is likely a bug in the game. We suggest reporting this as a GitHub issue. Details on the error are shown below. You can choose to create a new game or you can safely close Socially Distant.";

                _exceptionText.Text = _exceptionToHandle?.ToString() ??
                                      "No additional  information available to display. This is most likely also a bug.";

            _githubIssues.Text = "GitHub Issues";
            _newGame.Text = "New Game";

            _buttonList.Direction = StackDirection.Horizontal;

            _buttonList.Padding = 15;
            _newGame.Padding = 2;
            _githubIssues.Padding = 2;

            _newGame.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _githubIssues.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _exceptionMessageScroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _mainPanel.VerticalAlignment = VerticalAlignment.Center;

            _mainPanel.MaximumHeight = 600;

            _mainStacker.MaximumWidth = 800;
            _mainStacker.HorizontalAlignment = HorizontalAlignment.Center;

            _title.TextAlign = TextAlign.Center;
            _errorTitle.TextAlign = TextAlign.Center;
            _errorMessage.TextAlign = TextAlign.Center;

            _sociallyDistant.Image = Texture2D.FromResource(GamePlatform.GraphicsProcessor, this.GetType().Assembly,
                "SociallyDistant.Resources.LogoText.png");
            _sociallyDistant.ImageMode = ImageMode.Zoom;
            _sociallyDistant.FixedWidth = 320;
            _sociallyDistant.HorizontalAlignment = HorizontalAlignment.Center;

            _githubIssues.MouseUp += GithubIssuesOnMouseUp;
            _newGame.MouseUp += NewGameOnMouseUp;
            
            base.OnLoad();
        }

        private void NewGameOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                var sm = Game.GetComponent<SaveManager>();
                
                sm.DisarmPreloaderCrash();

                sm.CreateNew();

                GoToScene<BootScreen>();
            }
        }

        private void GithubIssuesOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                ThundershockPlatform.OpenFile("https://github.com/thundershock-alliance/socially-distant/issues");
            }
        }
    }
}