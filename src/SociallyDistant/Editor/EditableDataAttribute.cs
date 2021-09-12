using System;

namespace SociallyDistant.Editor
{
    public class EditableDataAttribute : Attribute
    {
        public string Name { get; set; }

        public EditableDataAttribute(string name)
        {
            Name = name;
        }
    }
}