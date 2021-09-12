using System;

namespace SociallyDistant.Editor
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EditorDescriptionAttribute : Attribute
    {
        public string Description { get; }

        public EditorDescriptionAttribute(string desc)
        {
            Description = desc;
        }
    }
}