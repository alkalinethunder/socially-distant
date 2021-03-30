using System;
using System.Collections.Generic;
using System.Linq;
using RedTeam.SaveData;
using Thundershock;
using Thundershock.Debugging;

namespace RedTeam.Net
{
    public class NetworkSimulation : SceneComponent
    {
        private SaveManager _saveManager;
        private MasterWebNode _masterWebNode = new MasterWebNode();
        
        public NetworkInterface GetNetworkInterface(Device device)
        {
            var net = _saveManager.GetDeviceNetwork(device);
            var isp = _saveManager.GetNetworkIsp(net);
            var reg = _saveManager.GetIspRegion(isp);

            var regNode = _masterWebNode.GetRegionNode(reg);
            var ispNode = regNode.GetIsp(isp);
            var netNode = ispNode.GetNetwork(net);
            var devNode = netNode.GetDevice(device);

            return new NetworkInterface(this, devNode);
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();
            _saveManager = App.GetComponent<SaveManager>();
            
            _saveManager.RegionAdded += AddRegionToMasterNode;
            _saveManager.IspAdded += AddIspNode;
            _saveManager.NetworkAdded += AddNetworkNode;
            _saveManager.DeviceAdded += AddDeviceNode;

            var cheater = App.GetComponent<CheatManager>();
            
            cheater.AddCheat("shownets", Cheat_ShowNets);
        }

        private void Cheat_ShowNets()
        {
            foreach (var node in _masterWebNode.CollapseNodes())
                App.Logger.Log(
                    $" - {node.Name} ({node.Address}, {NetworkHelpers.ToIPv4String(node.Address)}) type: {node.Type}");
        }

        private void AddDeviceNode(Device dev)
        {
            var net = _saveManager.GetDeviceNetwork(dev);
            var isp = _saveManager.GetNetworkIsp(net);
            var reg = _saveManager.GetIspRegion(isp);

            var regNode = _masterWebNode.GetRegionNode(reg);
            var ispNode = regNode.GetIsp(isp);
            var netNode = ispNode.GetNetwork(net);
            
            netNode.AddDevice(dev);
        }

        private void AddNetworkNode(Network net)
        {
            var isp = _saveManager.GetNetworkIsp(net);
            var region = _saveManager.GetIspRegion(isp);

            var regNode = _masterWebNode.GetRegionNode(region);

            var ispNode = regNode.GetIsp(isp);

            ispNode.AddNetwork(net);
        }

        private void AddIspNode(InternetServiceProvider isp)
        {
            var region = _masterWebNode.GetRegionNode(_saveManager.GetIspRegion(isp));
            region.AddIsp(isp);
        }

        private void AddRegionToMasterNode(RegionNetwork region)
        {
            _masterWebNode.AddRegion(region);
        }

        public int CalculateHops(WebNode source, WebNode dest)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (dest == null)
                throw new ArgumentNullException(nameof(dest));

            var nodes = _masterWebNode.CollapseNodes().ToArray();

            var prev = new int[nodes.Length];
            var dist = new int[prev.Length];

            var src = Array.IndexOf(nodes, source);
            
            var q = new List<WebNode>();

            for (var i = 0; i < dist.Length; i++)
            {
                q.Add(nodes[i]);
                dist[i] = int.MaxValue;
                prev[i] = int.MinValue;
            }

            dist[src] = 0;

            while (q.Any())
            {
                var u = q.OrderBy(x => dist[Array.IndexOf(nodes, x)]).First();
                q.Remove(u);
                var uIndex = Array.IndexOf(nodes, u);
                foreach (var connection in u.ConnectedNodes.Where(x => q.Contains(x)))
                {
                    var v = Array.IndexOf(nodes, connection);
                    var alt = dist[uIndex] + 1;

                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = uIndex;
                    }
                }    
            }

            var destIndex = Array.IndexOf(nodes, dest);
            
            return dist[destIndex];
        }
    }
}