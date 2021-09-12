using System;

namespace SociallyDistant.Editor
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EditorCategoryAttribute : Attribute
    {
        public string Category { get; }

        public EditorCategoryAttribute(string category)
        {
            Category = category;
        }
    }
}