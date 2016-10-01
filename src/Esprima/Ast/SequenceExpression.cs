using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SequenceExpression : Node,
        Expression
    {
        public IList<Expression> Expressions;

        public SequenceExpression(IList<Expression> expressions)
        {
            Type = Nodes.SequenceExpression;
            Expressions = expressions;
        }
    }
}