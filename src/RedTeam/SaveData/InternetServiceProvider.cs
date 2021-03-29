using System;

namespace RedTeam.SaveData
{
    public class InternetServiceProvider
    {
        public Guid Id = Guid.NewGuid();

        public Guid RegionId;
        
        public string Name;
        
        public uint SubnetMask;
        public uint NetworkAddress;
    }
}