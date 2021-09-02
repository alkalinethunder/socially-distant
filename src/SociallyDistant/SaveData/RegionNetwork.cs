using System;

namespace SociallyDistant.SaveData
{
    public class RegionNetwork
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }
        public uint SubnetMask { get; set; }
        public uint RegionAddress { get; set; }
    }
}