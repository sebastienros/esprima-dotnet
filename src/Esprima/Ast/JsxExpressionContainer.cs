using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JsxExpressionContainer : JsxExpression
    {
        public readonly Expression Expression;

        public JsxExpressionContainer(Expression expression) : base(Nodes.JSXExpressionContainer)
        {
            Expression = expression;
        }

        public override NodeCollection ChildNodes => new(Expression);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJsxExpressionContainer(this);
        }
    }
}
