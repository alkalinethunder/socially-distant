using System;

namespace SociallyDistant.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorTypeAttribute : Attribute
    {
        public Type Type { get; }

        public EditorTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}