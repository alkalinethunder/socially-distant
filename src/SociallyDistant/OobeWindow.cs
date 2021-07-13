using SociallyDistant.Core.Windowing;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant
{
    public class OobeWindow : Window
    {
        #region STATE

        private int _oobeState;
        private bool _done;
        
        #endregion
        
        #region GUI

        private Panel _bg = new();
        private Stacker _master = new();
        private TextBlock _title = new();
        private TextBlock _description = new();
        private ScrollPanel _scroller = new();
        private Stacker _buttonList = new();
        private Button _next = new();
        private Button _back = new();
        private CheckBox _agree = new();
        private TextBlock _agreeText = new();
        private TextEntry _username = new();
        private TextEntry _passwd = new();
        private TextEntry _passwdConfirm = new();
        private TextEntry _hostname = new();
        private TextBlock _passwdLabel = new();
        private TextBlock _usernameLabel = new();
        private TextBlock _hostnameLabel = new();
        private TextBlock _usernameError = new();
        private TextBlock _passwdError = new();
        private TextBlock _hostnameError = new();
        private Stacker _userStacker = new();
        private Stacker _eulaStacker = new();
        private Stacker _unameStacker = new();
        private Stacker _passwdStacker = new();
        private Stacker _hostnameStacker = new();
        
        #endregion

        public bool IsComplete => _done;
        public string Username => _username.Text;
        public string Password => _passwd.Text;
        public string Hostname => _hostname.Text;
        
        protected override void OnOpened()
        {
            BuildGui();

            SetupState();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _next.Enabled = _agree.IsChecked || _agree.Visibility != Visibility.Visible;
            _back.Enabled = _oobeState > 0;   
            base.OnUpdate(gameTime);
        }
        
        private void SetupState()
        {
            _agree.Visibility = Visibility.Collapsed;
            _back.Text = "Back";
            _next.Text = "Next";
            _scroller.Children.Clear();

            _usernameError.Visibility = Visibility.Collapsed;
            _passwdError.Visibility = Visibility.Collapsed;
            _hostnameError.Visibility = Visibility.Collapsed;
            
            
            switch (_oobeState)
            {
                case 0:
                    _title.Text = "Welcome";
                    _description.Text =
                        "Welcome to the Red-OS Initial Setup Wizard.  This setup wizard will guide you through creating your UNIX user account and setting core system preferences. Click Next to continue.";
                    break;
                case 1:
                    _title.Text = "License Agreement";
                    _description.Text =
                        "Your use of the Red-OS software is governed by the Agency End User License Agreement. Please carefully read and understand the below Terms and Conditions before continuing to use this operating system.";
                    _agree.Visibility = Visibility.Visible;
                    _scroller.Children.Add(_eulaStacker);
                    break;
                case 2:
                    _title.Text = "User Creation";
                    _description.Text = "Please choose a username, password, and hostname to use for your system.";
                    _scroller.Children.Add(_userStacker);
                    break;
                case 3:
                    if (!ValidateInformation())
                    {
                        _oobeState = 2;
                        goto case 2; // Truttle1 now HATES me.
                    }

                    _done = true;
                    Close();
                    
                    break;
            }
        }
        
        private void BuildGui()
        {
            _usernameError.ForeColor = Color.Red;
            _passwdError.ForeColor = Color.Red;
            _hostnameError.ForeColor = Color.Red;

            _usernameLabel.ForeColor = Color.White;
            _passwdLabel.ForeColor = Color.White;
            _hostnameLabel.ForeColor = Color.White;

            _usernameLabel.Text = "Username: ";
            _passwdLabel.Text = "Password: ";
            _hostnameLabel.Text = "Hostname: ";
            
            _title.ForeColor = Color.Cyan;
            _description.ForeColor = Color.White;
            
            _agreeText.Text = "I agree to the Terms and Conditions outlined above.";
            _agreeText.ForeColor = Color.White;
            
            _back.Padding = 2;
            _next.Padding = 2;

            FixedWidth = 620;
            FixedHeight = 460;
            
            _scroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _unameStacker.Direction = StackDirection.Horizontal;
            _passwdStacker.Direction = StackDirection.Horizontal;
            _hostnameStacker.Direction = StackDirection.Horizontal;

            _usernameLabel.Properties.SetValue(Stacker.FillProperty, new StackFill(1f / 3f));
            _passwdLabel.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _hostnameLabel.Properties.SetValue(Stacker.FillProperty, new StackFill(1f / 3f));

            _username.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _hostname.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _passwd.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _passwdConfirm.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _buttonList.HorizontalAlignment = HorizontalAlignment.Right;
            _buttonList.Direction = StackDirection.Horizontal;
            
            _bg.BackColor = Color.Black;
            _master.Padding = 10;
            _scroller.Padding = 5;
            _buttonList.Padding = 5;
            _title.Padding = new Padding(5, 5, 5, 2);
            _description.Padding = new Padding(5, 0, 5, 10);

            _unameStacker.Children.Add(_usernameLabel);
            _unameStacker.Children.Add(_username);

            _passwdStacker.Children.Add(_passwdLabel);
            _passwdStacker.Children.Add(_passwd);
            _passwdStacker.Children.Add(_passwdConfirm);
            
            _hostnameStacker.Children.Add(_hostnameLabel);
            _hostnameStacker.Children.Add(_hostname);
            
            _userStacker.Children.Add(_unameStacker);
            _userStacker.Children.Add(_usernameError);
            _userStacker.Children.Add(_passwdStacker);
            _userStacker.Children.Add(_passwdError);
            _userStacker.Children.Add(_hostnameStacker);
            _userStacker.Children.Add(_hostnameError);

            _agree.Children.Add(_agreeText);
            _buttonList.Children.Add(_back);
            _buttonList.Children.Add(_next);
            _master.Children.Add(_title);
            _master.Children.Add(_description);
            _master.Children.Add(_scroller);
            _master.Children.Add(_agree);
            _master.Children.Add(_buttonList);
            _bg.Children.Add(_master);
            Children.Add(_bg);
            
            _next.MouseUp += NextOnMouseUp;
            _back.MouseUp += BackOnMouseUp;
        }

        private bool ValidateInformation()
        {
            _username.Text = _username.Text.Trim();
            _passwd.Text = _passwd.Text.Trim();
            _passwdConfirm.Text = _passwdConfirm.Text.Trim();
            _hostname.Text = _hostname.Text.Trim();

            if (string.IsNullOrEmpty(_username.Text))
            {
                _usernameError.Visibility = Visibility.Visible;
                _usernameError.Text = "Username cannot be blank.";
                return false;
            }
            
            if (string.IsNullOrEmpty(_passwd.Text))
            {
                _passwdError.Visibility = Visibility.Visible;
                _passwdError.Text = "Password is required.";
                return false;
            }
            
            if (string.IsNullOrEmpty(_hostname.Text))
            {
                _hostnameError.Visibility = Visibility.Visible;
                _hostnameError.Text = "Hostname is required./";
                return false;
            }

            if (_passwd.Text != _passwdConfirm.Text)
            {
                _passwdError.Visibility = Visibility.Visible;
                _passwdError.Text = "Passwords do not match.";
                return false;
            }

            var unixUsername = Unixify(_username.Text);
            var unixHostname = Unixify(_hostname.Text);

            if (_username.Text != unixUsername)
            {
                _usernameError.Visibility = Visibility.Visible;
                _usernameError.Text = "Username contains invalid characters.";
                return false;
            }

            if (_hostname.Text != unixHostname)
            {
                _hostnameError.Visibility = Visibility.Visible;
                _hostnameError.Text = "Hostname contains invalid characters.";
                return false;
            }
            
            return true;
        }

        private string Unixify(string text)
        {
            var s = string.Empty;

            foreach (var ch in text)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    s += char.ToLower(ch);
                }
                else
                {
                    if (!s.EndsWith("-"))
                    {
                        s += "-";
                    }
                }
            }
            
            return s;
        }
        
        private void BackOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                _oobeState--;
                SetupState();
            }
        }

        private void NextOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                _oobeState++;
                SetupState();
            }
        }
    }
}