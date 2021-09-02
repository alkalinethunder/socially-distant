using SociallyDistant.SaveData;

namespace SociallyDistant.Net
{
    public class PortScanResult
    {
        private bool _isInLan;
        private Hackable _hackable;
        
        public PortScanResult(Hackable hackable, bool isInLan)
        {
            _hackable = hackable;
            _isInLan = isInLan;
        }

        public string Id => StaticGameDataRegistry.GetHackableId(_hackable.Type);
        public int Port => _hackable.Port;
        public string Status => GetStatus();

        private string GetStatus()
        {
            if (_hackable.HackableFlags.IsHacked)
                return "#cHACKED&0";
            
            if (_isInLan)
                return "open";

            return _hackable.HackableFlags.IsFirewalled ? "filtered" : "open";
        }
    }
}