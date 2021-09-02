using System;
using Thundershock.Core;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core.Gui.Elements
{
    public class LabeledDropdown : LayoutElement
    {
        private DropDownBox _dropdown = new();
        private TextBlock _title = new();
        private Stacker _stacker = new();

        public event EventHandler SelectedIndexChanged;
        
        public int SelectedIndex
        {
            get => _dropdown.SelectedIndex;
            set => _dropdown.SelectedIndex = value;
        }

        public string SelectedItem => _dropdown.SelectedItem;

        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }
        
        public LabeledDropdown()
        {
            _title.Properties.SetValue(FontStyle.Code);
            
            _stacker.Children.Add(_title);
            _stacker.Children.Add(_dropdown);
            
            _stacker.FixedWidth = 225;

            _dropdown.SelectedIndexChanged += DropdownOnSelectedIndexChanged;
            
            Children.Add(_stacker);
        }

        public void Clear() => _dropdown.Clear();
        public void AddItem(string item) => _dropdown.AddItem(item);
        public void RemoveItem(string item) => _dropdown.RemoveItem(item);
        
        private void DropdownOnSelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
        }
    }
}