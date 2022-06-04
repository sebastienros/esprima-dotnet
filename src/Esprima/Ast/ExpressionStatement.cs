using Esprima.Utils;

namespace Esprima.Ast
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) : base(Nodes.ExpressionStatement)
        {
            Expression = expression;
        }

        public sealed override NodeCollection ChildNodes => new(Expression);

        protected internal sealed override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExpressionStatement(this);
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
