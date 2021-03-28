using System;
using RedTeam.IO;
using Thundershock.IO;

namespace RedTeam.SaveData
{
    public class AgentController
    {
        private Agent _agent;
        private SaveManager _saveManager;
        private RedRoot _rootfs;
        
        public AgentController(SaveManager saveManager, Agent agent)
        {
            _saveManager = saveManager;
            _agent = agent;
            _rootfs = new RedRoot(this);
        }

        private Device Device => _saveManager.FindDeviceById(_agent.HomeDevice);
        private Identity Identity => _saveManager.FindIdentityById(_agent.Identity);

        public bool IsPlayer
            => _agent.AgentFlags.IsPlayer;

        public string DeviceId
            => Device.Id.ToString();
        
        public string UserName
            => Identity.Username;
        
        public string HostName
        {
            get => Device.HostName;
            set => Device.HostName = value ?? throw new ArgumentNullException(nameof(value));
        }

        public FileSystem CreateVfs()
        {
            return FileSystem.FromNode(_rootfs);
        }
    }
}