using System;

namespace SociallyDistant.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PropertyEditorAttribute : Attribute
    {
        public Type Type { get; }

        public PropertyEditorAttribute(Type type)
        {
            Type = type;
        }
    }
}