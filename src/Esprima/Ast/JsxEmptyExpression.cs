using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JsxEmptyExpression : JsxExpression
    {
        public JsxEmptyExpression() : base(Nodes.JSXEmptyExpression)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJsxEmptyExpression(this);
        }
    }
}
