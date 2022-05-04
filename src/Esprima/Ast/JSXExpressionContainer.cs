using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXExpressionContainer : JSXExpression
    {
        public readonly Expression Expression;

        public JSXExpressionContainer(Expression expression) : base(Nodes.JSXExpressionContainer)
        {
            Expression = expression;
        }

        public override NodeCollection ChildNodes => new(Expression);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXExpressionContainer(this);
        }
    }
}
