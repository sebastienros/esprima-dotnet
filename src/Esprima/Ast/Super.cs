using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Super : Expression
    {
        public Super() : base(Nodes.Super)
        {
        }

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}
