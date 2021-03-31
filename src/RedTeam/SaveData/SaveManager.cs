using System;
using System.Collections.Generic;
using System.Linq;
using RedTeam.Commands;
using RedTeam.Net;
using Thundershock;

namespace RedTeam.SaveData
{
    public class SaveManager : GlobalComponent
    {
        private SaveGame _currentGame;
        
        #region Events

        public event Action<RegionNetwork> RegionAdded;
        public event Action<InternetServiceProvider> IspAdded;
        public event Action<Network> NetworkAdded;
        public event Action<Device> DeviceAdded;
        public event Action<RegionNetwork, RegionNetwork> RegionLinked;
        #endregion
        
        public bool IsSaveAvailable
            => false; //TODO
        
        public bool IsLoaded
            => _currentGame != null;

        public bool IsPlayerReady
            => IsLoaded && _currentGame.PlayerAgent.AgentFlags.IsReady;

        private void ThrowIfPlayerReady()
        {
            ThrowIfNotLoaded();
            
            if (IsPlayerReady)
                throw new InvalidOperationException("The player's agent information has already been set.");
        }
        
        private void ThrowIfLoaded()
        {
            if (IsLoaded) throw new InvalidOperationException("A save game has already been loaded.");
        }

        private void ThrowIfNotLoaded()
        {
            if (!IsLoaded) throw new InvalidOperationException("Save game is not loaded.");
        }

        public void LoadGame()
        {
            throw new NotImplementedException();
        }

        public bool TryMapHostToAddress(string hostName, out uint address)
        {
            address = 0;
            var result = false;

            if (hostName == "localhost")
            {
                address = NetworkHelpers.LoopbackAddress;
                result = true;
            }
            else
            {
                if (_currentGame != null)
                {
                    var hostLookup = _currentGame.DnsEntries.FirstOrDefault(x => x.HostName == hostName);
                    if (hostLookup != null)
                    {
                        result = true;
                        address = hostLookup.Address;
                    }
                }
            }
            
            return result;
        }
        
        public void NewGame()
        {
            ThrowIfLoaded();
            
            _currentGame = new SaveGame();
            _currentGame.PlayerAgent = new Agent();
            _currentGame.PlayerAgent.AgentFlags.IsReady = false;
            _currentGame.PlayerAgent.AgentFlags.IsPlayer = true;
        }

        private void ThrowIfAgentNotReady(Agent agent)
        {
            if (!agent.AgentFlags.IsReady)
                throw new InvalidOperationException("Agent not ready.");
        }

        private AgentController CreateAgentController(Agent agent)
        {
            ThrowIfAgentNotReady(agent);

            return new AgentController(this, agent);
        }

        public Network GetDeviceNetwork(Device device)
        {
            ThrowIfNotLoaded();
            return _currentGame.Networks.First(x => x.Id == device.Network);
        }

        public IEnumerable<Device> GetNetworkDevices(Network network)
        {
            ThrowIfNotLoaded();
            
            foreach (var dev in _currentGame.Devices)
                if (dev.Network == network.Id)
                    yield return dev;
        }
        
        public Network CreateNetwork(InternetServiceProvider isp, NetworkType type, string displayName)
        {
            ThrowIfNotLoaded();
            
            var net = new Network();

            net.DisplayName = displayName;
            net.InternetServiceProviderId = isp.Id;
            net.SubnetMask = NetworkHelpers.NetworkSubnetMask;
            
            // set the network's local address
            var totalNetCount = _currentGame.Networks.Count;
            var localNetChooser = totalNetCount % NetworkHelpers.LocalNets.Length;
            net.SubnetAddress = NetworkHelpers.LocalNets[localNetChooser];
            
            // So here's the thing.
            // Region address assignment is easy.
            // ISP address assignment is easy.
            // NETWORK address assignment, on the other hand...
            // ...is harder than trying to date a raccoon.
            //
            // See, there's this thing in real life. This wonderful thing.
            // It's called Network Address Translation, or NAT.
            //
            // And we need to emulate that.
            //
            // Which means that we actually need to give the network TWO
            // IP addresses. One for it's local subnet - the LAN...
            // and another IP which is the public IP address. And the public address
            // is what the game's simulated Internet actually lets you talk to when
            // you're hacking in.
            //
            // Now, unlike ISPs and Region networks, Customer networks (the one
            // this code's generating right now) get 16 bits of space in the 4-byte IP address.
            // Whereas regions and ISPs get 8 bits.
            //
            // So here's what we've gotta do.
            //
            // First we need to count the number of networks in the ISP.
            var netCount = _currentGame.Networks.Aggregate(0,
                (acc, x) => acc = (x.InternetServiceProviderId == isp.Id) ? acc + 1 : acc);
            
            // And now, we need to know some rules about the game and how networking in it works.
            //
            // The last two bytes of an IP address in-game CANNOT both be 0 when we're in public space.
            var addressMin = 1;

            // And naturally it can't be larger than 255.255. Or, the max value of an unsigned short.
            var addressMax = ushort.MaxValue;
            
            // So this gives us a range of 65,533 values. But we're not done.
            //
            // Neither one of the bytes can ever be above 253.
            //
            // This takes care of the upper byte.
            addressMax -= 2;
            
            // And so we end up with this value.
            var address = addressMin + netCount;
            
            // Except that, again, neither of the two bytes can be above 253.
            if (address >= 253)
                address += 3; // skip 253, 254, 255.
            
            // So NOW we do the address max check.
            if (address > addressMax)
                throw new InvalidOperationException(
                    $"The ISP {isp.Name} has run out of free addresses in its IP range for customers. The max network limit has been reached.");
            
            // And NOW we get to set the address of the network.
            net.PublicAddress = isp.NetworkAddress | (uint) address;

            _currentGame.Networks.Add(net);

            NetworkAdded?.Invoke(net);
            
            return net;
        }

        public IEnumerable<RegionNetwork> GetRegionNeighbours(RegionNetwork region)
        {
            ThrowIfNotLoaded();

            foreach (var link in _currentGame.RegionLinks)
            {
                var n = default(Guid);

                if (link.RegionA == region.Id)
                    n = link.RegionB;
                else if (link.RegionB == region.Id)
                    n = link.RegionA;
                else continue;

                var regionNeighbour = _currentGame.Regions.FirstOrDefault(x => x.Id == n);
                if (regionNeighbour != null)
                    yield return regionNeighbour;
            }
        }

        public void RegisterDomainName(Network network, string domainName)
        {
            ThrowIfNotLoaded();

            if (_currentGame.DnsEntries.Any(x => x.HostName == domainName))
                throw new InvalidOperationException("Host already registered.");

            var dns = new DnsEntry
            {
                HostName = domainName,
                Address = network.PublicAddress
            };

            _currentGame.DnsEntries.Add(dns);
        }
        
        public void LinkRegions(RegionNetwork a, RegionNetwork b)
        {
            ThrowIfNotLoaded();

            if (a.Id != b.Id)
            {
                if (!_currentGame.RegionLinks.Any(x =>
                    (x.RegionA == a.Id && x.RegionB == b.Id) || (x.RegionB == a.Id && x.RegionA == b.Id)))
                {
                    var link = new RegionLInk
                    {
                        RegionA = a.Id,
                        RegionB = b.Id
                    };
                    _currentGame.RegionLinks.Add(link);

                    RegionLinked?.Invoke(a, b);
                }
            }
        }
        
        public InternetServiceProvider GetNetworkIsp(Network network)
        {
            ThrowIfNotLoaded();
            return _currentGame.ISPs.First(x => x.Id == network.InternetServiceProviderId);
        }

        public RegionNetwork GetIspRegion(InternetServiceProvider isp)
        {
            ThrowIfNotLoaded();
            return _currentGame.Regions.First(x => x.Id == isp.RegionId);
        }

        public RegionNetwork GetNetworkRegion(Network network)
        {
            return GetIspRegion(GetNetworkIsp(network));
        }

        public RegionNetwork CreateRegion(string name)
        {
            ThrowIfNotLoaded();

            var region = new RegionNetwork();
            region.Name = name;
            region.SubnetMask = NetworkHelpers.RegionSubnetMask;
            
            // Assign the region an IP address.
            // This will be a value between 11 and 240 not including 192.
            // 192 is reserved for local addresses.
            // By the end of this, if we end up with an address above 240,
            // the game's region limit has been reached.
            var addressMin = 11;
            var addressMax = 240;

            // address will be minimum + amount of already added regions
            var address = addressMin + _currentGame.Regions.Count;
            
            // skip 192.
            if (address >= 192)
                address++;
            
            // check if we've exceeded the maximum, and crash.
            if (address > addressMax)
                throw new InvalidOperationException(
                    "The game has run out of free regional IP Addresses. The game's regional limit has been exceeded.");
            
            // bit-shift the address to the left by 24 bits and assign it.
            region.RegionAddress = (uint) address << 24;
            
            _currentGame.Regions.Add(region);
            RegionAdded?.Invoke(region);
            return region;
        }

        public InternetServiceProvider CreateIsp(RegionNetwork region, string name)
        {
            ThrowIfNotLoaded();

            var isp = new InternetServiceProvider();

            isp.Name = name;
            isp.RegionId = region.Id;
            isp.SubnetMask = NetworkHelpers.IspSubnetMask;
            
            // Address assignment works basically the same way as for regions.
            // It'll be a value between 1 and 240 excluding 168.
            var addressMin = 1;
            var addressMax = 240;
            
            // Count the number of ISPs in this region.
            var ispCount = _currentGame.ISPs.Aggregate(0, (acc, x) => acc = (x.RegionId == region.Id) ? acc + 1 : acc);

            // That's the base address.
            var address = addressMin + ispCount;
            
            // skip 168
            if (address >= 168)
                address++;
            
            // check the maximum
            if (address > addressMax)
                throw new InvalidOperationException(
                    $"{region.Name} has run out of available IP addresses for ISP networks - the max ISP limit has been reached for this region.");
            
            // the final ISP address is the regional address, OR'd with the value we calculated above, bit shifted to the left by 16.
            isp.NetworkAddress = region.RegionAddress | ((uint) address << 16);
            
            _currentGame.ISPs.Add(isp);
            IspAdded?.Invoke(isp);
            return isp;
        }

        public Device CreateDevice(Network network, string hostname)
        {
            ThrowIfNotLoaded();

            var dev = new Device();

            dev.Network = network.Id;
            dev.HostName = hostname;

            // Device address assignment is extremely easy.
            var addressMin = 2;
            var addressMax = (~ network.SubnetMask) - addressMin;

            var devCount =
                _currentGame.Devices.Aggregate(0, (acc, x) => acc = (x.Network == network.Id) ? acc + 1 : acc);

            var address = addressMin + devCount;

            if (address > addressMax)
                throw new InvalidOperationException(
                    $"The network \"{network.DisplayName}\" has run out of local IP addresses. The maximum device limit for this network has been reached.");

            dev.LocalAddress = (uint) address;
            
            _currentGame.Devices.Add(dev);
            DeviceAdded?.Invoke(dev);
            return dev;
        }

        public Identity CreateIdentity(string username, string name)
        {
            ThrowIfNotLoaded();

            var id = new Identity();    
            id.FullName = name;
            id.Username = username;
            _currentGame.Identities.Add(id);
            return id;
        }

        public AgentController GetPlayerAgent()
        {
            ThrowIfNotLoaded();
            ThrowIfAgentNotReady(_currentGame.PlayerAgent);
            return CreateAgentController(_currentGame.PlayerAgent);
        }
        
        public void SetPlayerInfo(Identity identity, Device homeDevice)
        {
            ThrowIfPlayerReady();
            
            if (identity==null)
                throw new ArgumentNullException(nameof(identity));

            if (homeDevice == null)
                throw new ArgumentNullException(nameof(homeDevice));

            _currentGame.PlayerAgent.Identity = identity.Id;
            _currentGame.PlayerAgent.HomeDevice = homeDevice.Id;

            _currentGame.PlayerAgent.AgentFlags.IsReady = true;
        }

        public Device FindDeviceById(Guid guid)
        {
            ThrowIfNotLoaded();

            return _currentGame.Devices.First(x => x.Id == guid);
        }

        public Identity FindIdentityById(Guid guid)
        {
            ThrowIfNotLoaded();

            return _currentGame.Identities.First(x => x.Id == guid);
        }
    }
}