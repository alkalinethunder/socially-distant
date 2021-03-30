using System;

namespace RedTeam.Net
{
    public class NetworkManager
    {
        private IRedTeamContext _ctx;
        
        public NetworkManager(IRedTeamContext ctx)
        {
            _ctx = ctx;
        }

        public bool IsConnected
            => _ctx.Network != null;

        private void ThrowIfNotConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException("This device has no Internet connection.");
        }

        public string LocalAddress
        {
            get
            {
                ThrowIfNotConnected();
                return NetworkHelpers.ToIPv4String(_ctx.Network.LocalAddress);
            }
        }

        public string SubnetMask
        {
            get
            {
                ThrowIfNotConnected();
                return NetworkHelpers.ToCidrMask(_ctx.Network.NetworkAddress, _ctx.Network.SubnetMask);
            }
        }

        public bool GetPingTime(string host, out double ping, out string resolvedAddress)
        {
            var result = false;
            ping = 0;
            
            if (host == "localhost")
                host = "127.0.0.1"; // TODO: proper host lookups
            resolvedAddress = host;
            
            if (NetworkHelpers.TryParseIP(host, out uint addr))
            {
                var node = _ctx.Network.MapAddressToNode(addr);
                if (node != null)
                {
                    ping = _ctx.Network.GetHopsCount(node) * 2 + 24;
                    result = true;
                }
            }

            return result;
        }
    }
}