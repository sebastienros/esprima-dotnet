using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression) : base(Nodes.ExpressionStatement)
        {
            Expression = expression;
        }

        public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public sealed override NodeCollection ChildNodes => new(Expression);

        protected internal sealed override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitExpressionStatement(this, context);
        }

        protected virtual ExpressionStatement Rewrite(Expression expression)
        {
            return new ExpressionStatement(expression);
        }

        public ExpressionStatement UpdateWith(Expression expression)
        {
            if (expression == Expression)
            {
                return this;
            }

            return Rewrite(expression);
        }
    }
}
