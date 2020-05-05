using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public abstract class Node
    {
        protected static readonly IEnumerable<Node> ZeroChildNodes = Enumerable.Empty<Node>();

        protected Node(Nodes type) => Type = type;

        public Nodes Type { get; }
        public Range Range { get; set; }    

        public Location Location { get; set; }

        public abstract IEnumerable<Node> ChildNodes { get; }
    }
}
