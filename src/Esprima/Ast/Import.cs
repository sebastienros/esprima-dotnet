using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public class Import : Expression
    {
        public Import() : base(Nodes.Import)
        {
        }

        public override IEnumerable<Node> ChildNodes => Enumerable.Empty<Node>();
    }
}
