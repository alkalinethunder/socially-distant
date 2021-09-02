using System;

namespace SociallyDistant.SaveData
{
    public class Device
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DeviceFlags DeviceFlags { get; set; } = new();
        public string RootDirectory { get; set; }
        public string HostName { get; set; }

        public Guid Network { get; set; }
        public DeviceType DeviceType { get; set; }
        public uint LocalAddress { get; set; }
    }
}