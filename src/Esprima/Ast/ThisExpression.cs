using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ThisExpression : Expression
    {
        public ThisExpression() :
            base(Nodes.ThisExpression) {}

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}