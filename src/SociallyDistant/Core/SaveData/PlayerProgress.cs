using System;

namespace SociallyDistant.Core.SaveData
{
    public class PlayerProgress
    {
        public string FullName { get; set; }
        public Guid SafehouseId { get; set; }
        public Pronoun Pronoun { get; set; }
        
        public int Arrests { get; set; }
        public int Breaches { get; set; }
        public int Evades { get; set; }
        
        public long Cash { get; set; }
        
        public int Experience { get; set; }
        public int Skill { get; set; }
        public int Level { get; set; }
    }
}