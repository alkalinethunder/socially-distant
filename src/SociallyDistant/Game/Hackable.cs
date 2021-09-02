using System.Collections.Generic;
using SociallyDistant.Core.SaveData;

namespace SociallyDistant.Core.Game
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

    public class User
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string HomeDirectory { get; set; }
        public UserType Type { get; set; }

        public static User Root => new User
        {
            UserId = 0,
            UserName = "root",
            HomeDirectory = "/root",
            Type = UserType.Root
        };
        
        public static User Nobody => new User
        {
            UserId = int.MaxValue,
            UserName = "nobody",
            HomeDirectory = "/tmp",
            Type = UserType.Nobody
        };
    }

    public enum UserType
    {
        Root,
        SudoNoPasswd,
        Sudo,
        User,
        Nobody
    }
}