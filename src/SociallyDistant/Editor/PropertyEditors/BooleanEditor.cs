using System;
using SociallyDistant.Editor.Attributes;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editor.PropertyEditors
{
    [PropertyEditor(typeof(bool))]
    public sealed class BooleanEditor : AssetPropertyEditor<bool>
    {
        private CheckBox _checkBox = new();
        private TextBlock _text = new();

        protected override void Build()
        {
            _text.Text = "Enabled";
            _checkBox.Children.Add(_text);
            RootElement = _checkBox;
            
            _checkBox.CheckStateChanged += CheckBoxOnCheckStateChanged;
        }

        private void CheckBoxOnCheckStateChanged(object? sender, EventArgs e)
        {
            NotifyValueChanged(_checkBox.IsChecked);
        }

        protected override void OnValueChanged()
        {
            _checkBox.CheckState = Value ? CheckState.Checked : CheckState.Unchecked;
        }
    }
}