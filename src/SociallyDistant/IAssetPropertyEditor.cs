using System;
using System.Reflection;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core
{
    public interface IAssetPropertyEditor
    {
        object Value { get; set; }
        Element RootElement { get; }
        event EventHandler ValueChanged;
        void Initialize(object instance, PropertyInfo prop);
    }
}