using System;

namespace SociallyDistant.ContentEditors
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EditorNameAttribute : Attribute
    {
        public string Name { get; }

        public EditorNameAttribute(string name)
        {
            Name = name;
        }
    }
}