using System.Collections.Generic;
using System.IO;
using Thundershock.IO;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam.IO
{
    public class HomeNode : Node
    {
        private Node _parent;
        private AgentController _agent;

        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanCreate => true;
        public override bool CanDelete => false;
        public override bool CanExecute => false;
        public override bool CanList => true;

        public override Node Parent => _parent;

        public override IEnumerable<Node> Children
        {
            get
            {
                var hostDirectory = GetHostDirectory();
                
                foreach (var dir in Directory.GetDirectories(hostDirectory, "*", new EnumerationOptions()))
                {
                    yield return new HostDirectoryNode(this, dir);
                }

                foreach (var file in Directory.GetFiles(hostDirectory, "*", new EnumerationOptions()))
                {
                    yield return new HostFileNode(this, file);
                }
            }
        }

        public override string Name => _agent.UserName;

        public override Stream CreateFile(string name)
        {
            return File.Create(Path.Combine(GetHostDirectory(), name));
        }

        public override void CreateDirectory(string name)
        {
            Directory.CreateDirectory(Path.Combine(GetHostDirectory(), name));
        }

        public HomeNode(Node parent, AgentController agent)
        {
            _parent = parent;
            _agent = agent;
        }

        private string GetHostDirectory()
        {
            var homes = Path.Combine(ThundershockPlatform.LocalDataPath, "homes");
            var myHome = Path.Combine(homes, _agent.IsPlayer ? "player" : _agent.DeviceId);

            if (!Directory.Exists(homes))
                Directory.CreateDirectory(homes);

            if (!Directory.Exists(myHome))
                Directory.CreateDirectory(myHome);

            return myHome;
        }
    }
}