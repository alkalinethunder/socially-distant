using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Thundershock.Core;
using Thundershock.Debugging;
using Thundershock.IO;

namespace SociallyDistant.IO
{
    public class DebugNode : Node
    {
        private Node _parent;

        public DebugNode(Node parent)
        {
            _parent = parent;
        }

        public override bool CanDelete => false;
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanExecute => false;
        public override bool CanList => false;
        public override bool CanCreate => false;
        public override Node Parent => _parent;
        public override IEnumerable<Node> Children => Array.Empty<Node>();
        public override string Name => "debug";

        public override Stream Open(bool append)
        {
            Action<byte[]> submit = (bytes) =>
            {
                var str = Encoding.UTF8.GetString(bytes);

                var lines = str.Split(Environment.NewLine,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                    EntryPoint.CurrentApp.GetComponent<CheatManager>().ExecuteCommand(line);
            };

            return new SubmissionMemoryStream(Array.Empty<byte>(), submit);
        }
    }
}