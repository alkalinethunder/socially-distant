using System.Collections.Generic;
using System.Runtime.InteropServices;
using RedTeam.SaveData;

namespace RedTeam.Game
{
    public class HackableDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public HackableType Type { get; set; }
        public int DefaultPort { get; set; }
        public Difficulty MinimumDifficulty { get; set; }
        public List<DeviceType> PossibleDeviceTypes { get; set; }
    }
}