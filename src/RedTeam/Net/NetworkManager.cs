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

        public bool DnsLookup(string host, out uint address)
        {
            if (_ctx.Network == null)
            {
                address = 0;
                return false;
            }

            if (NetworkHelpers.TryParseIP(host, out uint parse))
            {
                address = parse;
                return true;
            }
            
            return _ctx.Network.DnsLookup(host, out address);
        }

        public bool GetPingTime(uint host, out double ping)
        {
            var result = false;
            ping = 0;

            var node = _ctx.Network.MapAddressToNode(host, out int hops);
            if (node != null)
            {
                ping = hops * 2 + 24;
                result = true;
            }

            return result;
        }
    }
}