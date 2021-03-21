using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedTeam.IO
{
    public abstract class Node
    {
        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public abstract bool CanExecute { get; }
        public abstract bool CanList { get; }
        public abstract bool CanCreate { get; }
        
        public abstract Node Parent { get; }
        
        public abstract IEnumerable<Node> Children { get; }

        public virtual long Length => Children.Aggregate(0l, (acc, x) => acc + x.Length);
        
        public abstract string Name { get; }

        public virtual Stream Open(bool append)
        {
            throw new NotSupportedException();
        }

        public virtual Stream CreateFile(string name)
        {
            throw new NotSupportedException();
        }
        
        public IEnumerable<Node> Collapse()
        {
            yield return this;
            foreach (var child in Children)
            {
                foreach (var sub in child.Collapse())
                    yield return sub;
            }
        }
    }
}