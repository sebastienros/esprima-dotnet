using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ChainExpression : Expression
    {
        public ChainExpression(Expression expression) : base(Nodes.ChainExpression)
        {
            Expression = expression;
        }

        /// <remarks>
        /// <see cref="CallExpression" /> | <see cref="ComputedMemberExpression" />| <see cref="StaticMemberExpression" />
        /// </remarks>
        public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Expression);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitChainExpression(this, context);
        }

        public ChainExpression UpdateWith(Expression expression)
        {
            if (expression == Expression)
            {
                return this;
            }

            return new ChainExpression(expression);
        }
    }
}
