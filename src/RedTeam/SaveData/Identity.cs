using System;

namespace RedTeam.SaveData
{
    public class Identity
    {
        public Guid Id = Guid.NewGuid();
        
        public string Username { get; set; }
        public string FullName { get; set; }
        public Pronoun GenderIdentity { get; set; }
    }
}