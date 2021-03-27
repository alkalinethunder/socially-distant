using System.Collections.Generic;
using RedTeam.SaveData;

namespace RedTeam.IO
{
    public class RedRoot : Node
    {
        private AgentController _agent;

        public RedRoot(AgentController agent)
        {
            _agent = agent;
        }

        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => true;
        public override bool CanDelete => false;
        public override bool CanCreate => false;
        public override string Name => "/";
        public override Node Parent => null;

        public override IEnumerable<Node> Children
        {
            get
            {
                yield return new DevFS(this);

                yield return new HomesMount(this, _agent);
            }
        }
    }
}