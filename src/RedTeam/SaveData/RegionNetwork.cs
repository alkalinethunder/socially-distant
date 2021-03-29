using System;

namespace RedTeam.SaveData
{
    public class RegionNetwork
    {
        public Guid Id = Guid.NewGuid();

        public string Name;
        public uint SubnetMask;
        public uint RegionAddress;
    }
}