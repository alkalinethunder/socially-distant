using System;

namespace SociallyDistant.Core.SaveData
{
    public class Hackable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeviceId { get; set; }
        public HackableFlags HackableFlags { get; set; } = new();
        public HackableType Type { get; set; }
        public Difficulty Difficulty { get; set; }
        public int Port { get; set; }
        public string NmapId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}