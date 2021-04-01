using System;

namespace RedTeam.SaveData
{
    public class Hackable
    {
        public Guid Id = Guid.NewGuid();
        public Guid DeviceId;
        public HackableFlags HackableFlags;
        public HackableType Type;
        public Difficulty Difficulty;
        public int Port;
        public string NmapId;
        public string Name;
        public string Description;
    }
}