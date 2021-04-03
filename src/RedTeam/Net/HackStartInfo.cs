using RedTeam.SaveData;
using Thundershock;

namespace RedTeam.Net
{
    public class HackStartInfo
    {
        private SaveManager _saveManager;
        private Hackable _hackable;
        private NetworkSimulation _netSim;
        private bool _finished;

        public bool IsFinished => _finished;
        
        public HackableType HackableType => _hackable.Type;
        
        public HackStartInfo(NetworkSimulation netSim, SaveManager saveManager, Hackable hackable)
        {
            _netSim = netSim;
            _saveManager = saveManager;
            _hackable = hackable;
        }

        public Difficulty Difficulty => _hackable.Difficulty;
        public bool IsTraced => _hackable.HackableFlags.IsTraced;
        public bool IsHacked => _hackable.HackableFlags.IsHacked;
        
        public void SetHacked()
        {
            _hackable.HackableFlags.IsHacked = true;
        }

        public IRedTeamContext CreateContext()
        {
            var device = _saveManager.FindDeviceById(_hackable.DeviceId);
            var nic = _netSim.GetNetworkInterface(device);

            var ctx = new HackContext(this, _saveManager, device, nic);

            return ctx;
        }

        public void Finish()
            => _finished = true;
    }
}