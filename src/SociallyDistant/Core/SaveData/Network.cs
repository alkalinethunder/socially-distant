using System;
using System.Collections.Generic;

namespace SociallyDistant.Core.SaveData
{
    public class Network
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InternetServiceProviderId { get; set; }
        
        public string DisplayName { get; set; }
        public NetworkFlags NetworkFlags { get; set; } = new();
        public uint SubnetMask { get; set; }
        public uint SubnetAddress { get; set; }
        public uint PublicAddress { get; set; }
        public List<NetworkPortMapEntry> PortMappings { get; set; } = new();
        public NetworkType NetworkType { get; set; }
    }
}