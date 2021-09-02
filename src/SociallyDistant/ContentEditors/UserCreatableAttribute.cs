using System;

namespace SociallyDistant.ContentEditors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UserCreatableAttribute : Attribute
    {
        public bool IsUserCreateable { get; }

        public UserCreatableAttribute(bool value)
        {
            IsUserCreateable = value;
        }
    }
}