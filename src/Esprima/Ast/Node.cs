using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public abstract class Node : INode
    {
        public Nodes Type { get; }
        public Range Range { get; set; }

        public Location Location { get; set; }

        public abstract IEnumerable<INode> ChildNodes { get; }

        protected static IEnumerable<INode> ZeroChildNodes = Enumerable.Empty<INode>();

        protected Node(Nodes type) =>
            Type = type;
    }
}
