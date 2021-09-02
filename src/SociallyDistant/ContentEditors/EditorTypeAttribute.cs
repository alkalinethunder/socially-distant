using System;

namespace SociallyDistant.Core.ContentEditors
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