using System.Collections.Generic;

namespace SociallyDistant.Core.Net
{
    public abstract class WebNode
    {
        public abstract IEnumerable<WebNode> ConnectedNodes { get; }
        public abstract WebNodeType Type { get; }
        
        public abstract string Name { get; }
        public abstract uint Address { get; }
    }
}