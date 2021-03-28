using System.Collections.Generic;
using Thundershock.IO;
using RedTeam.SaveData;

namespace RedTeam.IO
{
    public class HomesMount : Node
    {
        private Node _parent;
        private AgentController _agent;

        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanCreate => false;
        public override bool CanList => true;
        public override bool CanDelete => false;
        public override string Name => "home";

        public override IEnumerable<Node> Children
        {
            get
            {
                yield return new HomeNode(this, _agent);
            }
        }

        public override Node Parent => _parent;
        
        public HomesMount(Node parent, AgentController agent)
        {
            _agent = agent;
            _parent = parent;
        }
    }
}