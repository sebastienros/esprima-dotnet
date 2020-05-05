using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public Import() : base(Nodes.Import)
        {
        }

        public override IEnumerable<Node> ChildNodes => Enumerable.Empty<Node>();
    }
}
