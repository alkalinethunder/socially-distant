using System;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class SettingToggle : LayoutElement
    {
        private CheckBox _checkBox = new();
        private TextBlock _text = new();
        private Stacker _stacker = new();
        private TextBlock _checkLabel = new();

        public string Title
        {
            get => _text.Text;
            set => _text.Text = value;
        }

        public string CheckLabel
        {
            get => _checkLabel.Text;
            set => _checkLabel.Text = value;
        }

        public bool IsChecked
        {
            get => _checkBox.IsChecked;
            set => _checkBox.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
        }

        public event EventHandler CheckStateChanged;
        
        public SettingToggle()
        {
            _text.Properties.SetValue(FontStyle.Code);
            
            _stacker.FixedWidth = 225;
            
            _checkLabel.Text = "Enabled";
            
            _checkBox.Children.Add(_checkLabel);
            _stacker.Children.Add(_text);
            _stacker.Children.Add(_checkBox);

            _checkBox.CheckStateChanged += CheckBoxOnCheckStateChanged;
            
            Children.Add(_stacker);
        }

        private void CheckBoxOnCheckStateChanged(object sender, EventArgs e)
        {
            CheckStateChanged?.Invoke(this, e);
        }
    }
}