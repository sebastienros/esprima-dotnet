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

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}
