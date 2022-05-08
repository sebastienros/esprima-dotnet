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

        protected internal sealed override void Accept(AstVisitor visitor)
        {
            visitor.VisitExpressionStatement(this);
        }
    }
}
