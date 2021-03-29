using System;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam.Net
{
    public class NetworkSimulation : SceneComponent
    {
        private SaveManager _saveManager;

        public NetworkInterface GetNetworkInterface(Device device)
        {
            return new NetworkInterface(this, device, _saveManager.GetDeviceNetwork(device));
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();
            _saveManager = App.GetComponent<SaveManager>();
        }
    }
}