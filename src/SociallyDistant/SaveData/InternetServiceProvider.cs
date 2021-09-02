using System;

namespace SociallyDistant.Core.SaveData
{
    public class InternetServiceProvider
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RegionId { get; set; }
        
        public string Name { get; set; }
        
        public uint SubnetMask { get; set; }
        public uint NetworkAddress { get; set; }
    }
}