using System;

namespace SociallyDistant.SaveData
{
    public class Identity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Username { get; set; }
        public string FullName { get; set; }
        public Pronoun GenderIdentity { get; set; }
    }
}