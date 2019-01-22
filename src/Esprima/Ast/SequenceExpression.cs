using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SequenceExpression : Node, Expression
    {
        public List<Expression> Expressions { get; internal set; }

        public SequenceExpression(List<Expression> expressions) :
            base(Nodes.SequenceExpression)
        {
            Expressions = expressions;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Expressions);
    }
}