using System;
using System.Collections.Generic;

namespace RedTeam.SaveData
{
    public class Network
    {
        public Guid Id = Guid.NewGuid();

        public Guid InternetServiceProviderId;
        
        public string DisplayName;
        public NetworkFlags NetworkFlags;
        public uint SubnetMask;
        public uint SubnetAddress;
        public uint PublicAddress;
        public List<NetworkPortMapEntry> PortMappings = new();
        public NetworkType NetworkType;
    }
}