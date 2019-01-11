using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Super : Node, Expression
    {
        public Super()
        {
            Type = Nodes.Super;
        }

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
    }
}
