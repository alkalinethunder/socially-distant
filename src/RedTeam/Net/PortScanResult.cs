using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class PortScanResult
    {
        private bool _isInLAN = false;
        private Hackable _hackable;
        
        public PortScanResult(Hackable hackable, bool isInLAN)
        {
            _hackable = hackable;
            _isInLAN = isInLAN;
        }

        public string Id => StaticGameDataRegistry.GetHackableId(_hackable.Type);
        public int Port => _hackable.Port;
        public string Status => GetStatus();

        private string GetStatus()
        {
            if (_hackable.HackableFlags.IsHacked)
                return "#cHACKED&0";
            
            if (_isInLAN)
                return "open";

            return _hackable.HackableFlags.IsFirewalled ? "filtered" : "open";
        }
    }
}