using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SequenceExpression : Node, Expression
    {
        public List<Expression> Expressions { get; internal set; }

        public SequenceExpression(List<Expression> expressions)
        {
            Type = Nodes.SequenceExpression;
            Expressions = expressions;
        }
    }
}