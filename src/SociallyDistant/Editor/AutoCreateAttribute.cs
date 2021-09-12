using System;

namespace SociallyDistant.Editor
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