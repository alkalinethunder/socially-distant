using RedTeam.SaveData;

namespace RedTeam.Net
{
    public class NetworkInterface
    {
        private NetworkSimulation _simulation;
        private Device _device;
        private Network _network;
        public NetworkInterface(NetworkSimulation simulation, Device device, Network network)
        {
            _simulation = simulation;
            _device = device;
            _network = network;
        }

        public uint LocalAddress
            => (NetworkAddress & SubnetMask) | (_device.LocalAddress & ~SubnetMask);

        public uint SubnetMask
            => _network.SubnetMask;

        public uint PublicAddress
            => _network.PublicAddress;

        public uint NetworkAddress
            => _network.SubnetAddress;
    }
}