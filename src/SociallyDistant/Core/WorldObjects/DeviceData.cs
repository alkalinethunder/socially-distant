using System;
using System.Collections.Generic;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    public class DeviceData : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakName("Device Name")]
        [TweakDescription("Because this is still a game, this value determines what the device will show up as in the game's UI.")]
        public string Name { get; set; }
        
        [TweakName("Host Name")]
        [TweakDescription("Optional UNIX hostname to write to the device's /etc/hostname file. If left blank, the game will use localhost.")]
        public string HostName { get; set; }
        
        [TweakName("Device Type")]
        [TweakDescription("Defines the type of the device and what types of hackable services will spawn.")]
        public DeviceType DeviceType { get; set; }
        
        [TweakHidden] public List<string> Users { get; set; } = new();

        public User GetUser(int uid)
        {
            if (uid == 0)
                return User.Root;

            uid--;

            if (uid < 0 || uid >= this.Users.Count)
                return User.Nobody;

            return new User
            {
                UserId = uid + 1,
                UserName = this.Users[uid],
                HomeDirectory = "/home/" + this.Users[uid],
                Type = UserType.User
            };
        }
    }
}