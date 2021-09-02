using System;

namespace SociallyDistant.ContentEditors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoCreateAttribute : Attribute
    {
        public string Name { get; }

        public AutoCreateAttribute(string name)
        {
            Name = name;
        }
    }
}