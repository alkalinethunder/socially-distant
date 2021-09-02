using System;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomEditorAttribute : Attribute
    {
        public Type Type { get; }

        public CustomEditorAttribute(Type type)
        {
            if (!typeof(Element).IsAssignableFrom(type))
                throw new InvalidOperationException("Type must be of a GUI element.");

            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw new InvalidOperationException("Type must have a parameterless constructor.");

            Type = type;
        }
    }
}