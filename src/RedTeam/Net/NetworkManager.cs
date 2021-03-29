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
    }
}