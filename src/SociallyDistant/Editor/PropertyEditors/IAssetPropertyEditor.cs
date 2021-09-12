using System;
using System.Reflection;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editor.PropertyEditors
{
    public interface IAssetPropertyEditor
    {
        object Value { get; set; }
        Element RootElement { get; }
        event EventHandler ValueChanged;
        void Initialize(object instance, PropertyInfo prop);
    }
}