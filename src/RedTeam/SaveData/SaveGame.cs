using System.Collections.Generic;

namespace RedTeam.SaveData
{
    public class SaveGame
    {
        public Agent PlayerAgent;
        public List<Agent> NonPlayerAgents = new List<Agent>();
        public List<Device> Devices = new List<Device>();
        public List<Identity> Identities = new List<Identity>();
    }
}