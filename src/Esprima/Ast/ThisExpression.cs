using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public class ThisExpression : Node,
        Expression
    {
        public ThisExpression() :
            base(Nodes.ThisExpression) {}

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
    }
}