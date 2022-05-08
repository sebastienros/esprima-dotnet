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

        protected internal sealed override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitExpressionStatement(this) as T;
        }
    }
}
