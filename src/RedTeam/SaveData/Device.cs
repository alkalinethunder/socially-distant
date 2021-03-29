using System;

namespace RedTeam.SaveData
{
    public class Device
    {
        public Guid Id = Guid.NewGuid();
        
        public string RootDirectory { get; set; }
        public string HostName { get; set; }

        public Guid Network;
        
        public uint LocalAddress;
    }
}