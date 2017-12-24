using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SequenceExpression : Node, Expression
    {
        public List<INode> Expressions { get; internal set; }

        public SequenceExpression(List<INode> expressions)
        {
            Type = Nodes.SequenceExpression;
            Expressions = expressions;
        }
    }
}