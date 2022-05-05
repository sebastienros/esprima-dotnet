using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SequenceExpression : Expression
    {
        internal NodeList<Expression> _expressions;

        public SequenceExpression(in NodeList<Expression> expressions) : base(Nodes.SequenceExpression)
        {
            _expressions = expressions;
        }

        public ReadOnlySpan<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _expressions.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_expressions);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitSequenceExpression(this);
        }

        public SequenceExpression UpdateWith(in NodeList<Expression> expressions)
        {
            if (NodeList.AreSame(expressions, _expressions))
            {
                return this;
            }

            return new SequenceExpression(expressions);
        }
    }
}
