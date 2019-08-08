using System.Collections.Generic;

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