using System;

namespace SociallyDistant.Core.ContentEditors
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