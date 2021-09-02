using System;
using System.Linq;
using SociallyDistant.Gui.Elements;
using SociallyDistant.SaveData;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant
{
    public class Oobe
    {
        #region Scenes

        private BootScreen _boot;

        #endregion

        #region Components

        private SaveManager _saveManager;
        private GuiSystem Gui => _boot.Gui;

        #endregion

        #region Gui

        private Panel _overlay = new();
        private Stacker _mainStacker = new();
        private Picture _logo = new();
        private TextBlock _title = new();
        private TextBlock _prompt = new();
        private ScrollPanel _scroller = new();
        private Button _continue = new();
        
        #endregion

        #region State

        private Pronoun _activePronoun;
        private int _page;
        private Func<bool> _validator;
        private bool _transitioning;
        private int _transState;
        private float _transProgress;
        
        #endregion
        
        #region GUI - Page 1

        private Stacker _pronounSelect = new();
        private Stacker _pronounEdit = new();
        private DropDownBox _pronoun = new();
        private ChatLayout _chatLayout = new();

        #endregion

        #region GUI - Page 2

        private TextEntry _fullName = new();

        #endregion

        #region Gui - Page 3

        private ProgressBar _progress = new();

        #endregion
        
        internal Oobe(BootScreen boot)
        {
            _boot = boot;

            _saveManager = _boot.Game.GetComponent<SaveManager>();
            
            BuildGui();
            UpdateGui();
        }
        
        public void Update(float deltaTime)
        {
            if (_validator != null)
            {
                _continue.Enabled = _validator();
            }
            else
            {
                _continue.Enabled = true;
            }

            if (_page == 3 && !_transitioning)
            {
                _progress.Value = MathHelper.Clamp(_progress.Value + (deltaTime / 5), 0, 1);
                if (_progress.Value >= 1)
                {
                    StartTransition();
                }
            }

            if (_transitioning)
            {
                switch (_transState)
                {
                    case 0:
                        _overlay.Opacity = MathHelper.Clamp(1 - _transProgress, 0, 1);
                        _transProgress += deltaTime * 2;
                        if (_transProgress >= 1)
                        {
                            _overlay.Opacity = 0;
                            _transState++;
                        }
                        break;
                    case 1:
                        _page++;
                        UpdateGui();
                        _transProgress = 0;
                        _transState++;
                        break;
                    case 2:
                        _overlay.Opacity = MathHelper.Clamp(_transProgress, 0, 1);
                        _transProgress += deltaTime * 2;
                        if (_transProgress >= 1)
                        {
                            _overlay.Opacity = 1;
                            _transitioning = false;
                        }
                        break;
                }
            }
        }

        private void BuildGui()
        {
            _mainStacker.Children.Add(_logo);
            _mainStacker.Children.Add(_title);
            _mainStacker.Children.Add(_prompt);
            _mainStacker.Children.Add(_scroller);
            _mainStacker.Children.Add(_continue);

            _overlay.Children.Add(_mainStacker);
            Gui.AddToViewport(_overlay);
            
            _logo.FixedHeight = 96;
            _logo.HorizontalAlignment = HorizontalAlignment.Left;
            _logo.ImageMode = ImageMode.Fit;
            _logo.Image = _saveManager.ContentPack.BootLogo;
            
            _continue.Text = "Continue >> ";
            
            _overlay.BackColor = Color.Black * 0.333333f;
            _overlay.VerticalAlignment = VerticalAlignment.Center;

            _mainStacker.Padding = 20;

            _title.ForeColor = Color.Cyan;
            _title.Properties.SetValue(FontStyle.Heading1);
            
            _continue.HorizontalAlignment = HorizontalAlignment.Left;

            _title.Padding = new Padding(0, 1 * _prompt.FontSize);
            _scroller.Padding = new Padding(0, 2 * _prompt.FontSize);
            
            _continue.MouseUp += ContinueOnMouseUp;
            
            _pronounEdit.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _chatLayout.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _pronounSelect.Children.Add(_pronounEdit);
            _pronounSelect.Children.Add(_chatLayout);

            _pronounEdit.Children.Add(_pronoun);

            _pronoun.AddItem("Male (He/Him)");
            _pronoun.AddItem("Female (She/Her)");
            _pronoun.AddItem("Non-binary (They/Them)");
            _pronoun.SelectedIndex = 0;
            
            _pronoun.SelectedIndexChanged += PronounOnSelectedIndexChanged;

            _pronounEdit.VerticalAlignment = VerticalAlignment.Center;
            _chatLayout.Padding = 15;

            _progress.HorizontalAlignment = HorizontalAlignment.Left;
            _progress.FixedWidth = 460;

            _pronounSelect.Direction = StackDirection.Horizontal;
        }

        private void PronounOnSelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdatePronounDisplay();
        }

        private void ContinueOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (!_transitioning)
                StartTransition();
        }

        private void StartTransition()
        {
            _transitioning = true;
            _transState = 0;
            _transProgress = 0;
        }

        private void UpdateGui()
        {
            _validator = null;
            _scroller.Children.Clear();

            switch (_page)
            {
                case 0:
                    _title.Text = "Welcome";
                    _prompt.Text = $"Welcome to {_saveManager.ContentPack.Name}. Enter your name to continue.";

                    _scroller.Children.Add(_fullName);

                    _validator = () => !string.IsNullOrWhiteSpace(_fullName.Text);
                    
                    break;
                case 1:
                    _title.Text = "Pronoun Select";
                    _prompt.Text = "Choose which pronouns other people should use when communicating with you.";

                    _scroller.Children.Add(_pronounSelect);

                    _chatLayout.ConversationName = "Group DM";
                    _chatLayout.ParticipantsText = "Two participants";

                    UpdatePronounDisplay();
                    
                    break;
                case 2:
                    _title.Text = $"Welcome, {_fullName.Text}.";
                    _prompt.Text = "Press [Continue] to begin setting up your desktop.";
                    break;
                case 3:
                    _continue.Visibility = Visibility.Collapsed;

                    _title.Text = "Please wait...";
                    _prompt.Text = "Your virtual machine is being provisioned.";
                    
                    _scroller.Children.Add(_progress);

                    break;
                case 4:
                    _overlay.RemoveFromParent();
                    _saveManager.CurrentGame.AutoCreatePlayer(_fullName.Text, _activePronoun);
                    _saveManager.Save();
                    break;
            }
        }

        private void UpdatePronounDisplay()
        {
            _chatLayout.ClearMessages();

            var pronoun = _pronoun.SelectedIndex switch
            {
                0 => Pronoun.Male,
                1 => Pronoun.Female,
                2 => Pronoun.Unisex
            };

            _activePronoun = pronoun;

            var m1 = $"Hey, have you met {_fullName.Text}?";
            var m2 = $"Yeah, {Localization.GetPronounText(pronoun, PronounUsage.TheyAre)} pretty cool.";
            var m3 =
                $"Really? {Localization.GetPronounText(pronoun, PronounUsage.They, "seem").Capitalize()} a little... Ehhh... interesting...";
            var m4 =
                $"I guess. I suppose I wouldn't wanna be on {Localization.GetPronounText(pronoun, PronounUsage.Their)} bad side.";
            var m5 = $"Yeah. I wouldn't piss {Localization.GetPronounText(pronoun, PronounUsage.Them)} off.";

            _chatLayout.AddMessage(m1, false);
            _chatLayout.AddMessage(m2, true);
            _chatLayout.AddMessage(m3, false);
            _chatLayout.AddMessage(m4, true);
            _chatLayout.AddMessage(m5, false);
        }
    }

    public enum PronounUsage
    {
        Them,
        They,
        Their,
        TheyAre
    }

    public static class Localization
    {
        public static string GetPronounText(Pronoun pronoun, PronounUsage usage)
        {
            switch (pronoun)
            {
                case Pronoun.Male:
                    return usage switch
                    {
                        PronounUsage.Them => "him",
                        PronounUsage.They => "he",
                        PronounUsage.Their => "his",
                        PronounUsage.TheyAre => "he's",
                    };
                case Pronoun.Female:
                    return usage switch
                    {
                        PronounUsage.Them => "her",
                        PronounUsage.They => "she",
                        PronounUsage.Their => "her",
                        PronounUsage.TheyAre => "she's",
                    };
                case Pronoun.Unisex:
                    return usage switch
                    {
                        PronounUsage.Them => "them",
                        PronounUsage.They => "they",
                        PronounUsage.Their => "their",
                        PronounUsage.TheyAre => "they're",
                    };
            }

            return string.Empty;
        }

        public static string GetPronounText(Pronoun pronoun, PronounUsage usage, string word)
        {
            var prefix = GetPronounText(pronoun, usage);

            if (pronoun != Pronoun.Unisex)
                word += "s";

            return $"{prefix} {word}";
        }

        public static string Capitalize(this string text)
        {
            return $"{char.ToUpper(text[0])}{text.Substring(1)}";
        }
    }
}