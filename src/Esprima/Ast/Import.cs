using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public class Import : Node, Expression
    {
        public Import() : base(Nodes.Import)
        {
        }

        public override IEnumerable<INode> ChildNodes => Enumerable.Empty<INode>();
    }
}
