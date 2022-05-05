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

        public override NodeCollection ChildNodes => new(Expression);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitExpressionStatement(this) as T;
        }
    }
}
