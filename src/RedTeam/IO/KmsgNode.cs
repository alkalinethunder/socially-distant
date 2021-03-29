using System;
using System.Collections.Generic;
using System.IO;
using Thundershock.IO;

namespace RedTeam.IO
{
    public class KmsgNode : Node
    {
        private Node _parent;

        public KmsgNode(Node parent)
        {
            _parent = parent;
        }
        
        public override bool CanDelete => false;
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanExecute => false;
        public override bool CanList => false;
        public override bool CanCreate => false;
        public override Node Parent => _parent;
        public override IEnumerable<Node> Children => Array.Empty<Node>();
        public override string Name => "kmsg";

        public override Stream Open(bool append)
        {
            return KmsgLogOutput.OpenLogStream();
        }
    }
}