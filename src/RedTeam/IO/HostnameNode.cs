using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RedTeam.SaveData;

namespace RedTeam.IO
{
    public class HostnameNode : Node
    {
        private Node _parent;
        private AgentController _agent;

        public HostnameNode(Node parent, AgentController agent)
        {
            _parent = parent;
            _agent = agent;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanExecute => false;
        public override bool CanList => false;
        public override bool CanCreate => false;
        public override bool CanDelete => false;

        public override Node Parent => _parent;
        public override IEnumerable<Node> Children => Array.Empty<Node>();
        public override string Name => "hostname";

        public override Stream Open(bool append)
        {
            // encode the data of the agent's hostname to binary (utf8)
            var enc = Encoding.UTF8.GetBytes(_agent.HostName);
            
            // submission function
            Action<byte[]> submit = (bytes) =>
            {
                var raw = Encoding.UTF8.GetString(bytes).Trim();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    var lines = raw.Split(Environment.NewLine);
                    _agent.HostName = lines.First().Trim();
                }
            };
            
            // create a submission stream
            var stream = new SubmissionMemoryStream(enc, submit);

            if (append)
                stream.Position = stream.Length;

            return stream;
        }
    }
}