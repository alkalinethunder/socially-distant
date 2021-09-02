using System;

namespace SociallyDistant.SaveData
{
    public class Agent
    {
        public AgentFlags AgentFlags { get; set; } = new();
        public Guid HomeDevice { get; set; }
        public Guid Identity { get; set; }
    }
}