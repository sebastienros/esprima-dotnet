using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SequenceExpression : Node, Expression
    {
        internal NodeList<Expression> _expressions;

        public SequenceExpression(in NodeList<Expression> expressions) :
            base(Nodes.SequenceExpression)
        {
            _expressions = expressions;
        }

        public ref readonly NodeList<Expression> Expressions => ref _expressions;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Expressions);
    }
}