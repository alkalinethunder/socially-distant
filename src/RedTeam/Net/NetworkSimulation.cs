using System;
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
            return new NetworkInterface(this, device, _saveManager.GetDeviceNetwork(device));
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
    }
}