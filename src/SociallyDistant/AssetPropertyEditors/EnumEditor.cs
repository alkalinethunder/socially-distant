using System;
using System.Reflection;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core.AssetPropertyEditors
{
    public sealed class EnumEditor : IAssetPropertyEditor
    {
        private int _value;
        private object _instance;
        private DropDownBox _selector = new();
        private PropertyInfo _prop;

        public object Value
        {
            get => _value;
            set
            {
                _value = (int) value;
                _selector.SelectedIndex = _value;
            }
        }

        public Element RootElement => _selector;
        
        public event EventHandler ValueChanged;
        
        public void Initialize(object instance, PropertyInfo prop)
        {
            if (!prop.PropertyType.IsEnum)
                throw new InvalidOperationException("Type must be an enum.");

            _prop = prop;
            _instance = instance;
            _value = (int) prop.GetValue(instance);

            foreach (var name in Enum.GetNames(prop.PropertyType))
            {
                _selector.AddItem(name);
            }

            _selector.SelectedIndex = _value;
            
            _selector.SelectedIndexChanged += SelectorOnSelectedIndexChanged;
        }

        private void SelectorOnSelectedIndexChanged(object? sender, EventArgs e)
        {
            _value = _selector.SelectedIndex;
            ValueChanged?.Invoke(this, e);

            _prop.SetValue(_instance, _value);
        }
    }
}