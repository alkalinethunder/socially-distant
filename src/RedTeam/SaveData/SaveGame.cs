using System.Collections.Generic;

namespace RedTeam.SaveData
{
    public class SaveGame
    {
        public Agent PlayerAgent;
        public List<Agent> NonPlayerAgents = new List<Agent>();
        public List<Device> Devices = new List<Device>();
        public List<Identity> Identities = new List<Identity>();
        public List<Network> Networks = new List<Network>();
        public List<InternetServiceProvider> ISPs = new();
        public List<RegionNetwork> Regions = new List<RegionNetwork>();
        public List<RegionLInk> RegionLinks = new();
    }
}