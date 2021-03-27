using System;
using System.Linq;
using RedTeam.Commands;

namespace RedTeam.SaveData
{
    public class SaveManager : GlobalComponent
    {
        private SaveGame _currentGame;

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

        public Device CreateDevice(string hostname)
        {
            ThrowIfNotLoaded();

            var dev = new Device();

            dev.HostName = hostname;
            
            _currentGame.Devices.Add(dev);
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
        
        public AgentContext CreatePlayerContexxt()
        {
            ThrowIfNotLoaded();
            ThrowIfAgentNotReady(_currentGame.PlayerAgent);

            return new AgentContext(CreateAgentController(_currentGame.PlayerAgent));
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