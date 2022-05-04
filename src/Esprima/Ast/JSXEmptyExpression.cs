using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXEmptyExpression : JSXExpression
    {
        public JSXEmptyExpression() : base(Nodes.JSXEmptyExpression)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXEmptyExpression(this);
        }
    }
}
