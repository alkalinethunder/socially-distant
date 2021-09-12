using System;
using System.Reflection;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editor.PropertyEditors
{
    public abstract class AssetPropertyEditor<T> : IAssetPropertyEditor
    {
        private T _value;
        private object _instance;
        private Element _root;
        private PropertyInfo _prop;
        
        object IAssetPropertyEditor.Value
        {
            get => _value;
            set => this.Value = (T) value;
        }
        
        protected PropertyInfo Property { get; private set; }

        public Element RootElement
        {
            get => _root;
            set => _root = value;
        }

        public event EventHandler ValueChanged;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged();
            }
        }

        protected void NotifyValueChanged(T value)
        {
            _value = value;
            _prop.SetValue(_instance, _value);
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void Build();
        
        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Initialize(object instance, PropertyInfo prop)
        {
            if (!prop.PropertyType.IsAssignableTo(typeof(T)) && (typeof(T) != typeof(object)))
                throw new InvalidOperationException("Incompatible property type.");

            _prop = prop;
            _instance = instance;
            this.Property = prop;

            var value = (T) prop.GetValue(_instance);
            this.Value = value;

            this.Build();
        }
    }
}