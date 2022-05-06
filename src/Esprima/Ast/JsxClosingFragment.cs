using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JsxClosingFragment : JsxExpression
    {

        public JsxClosingFragment() : base(Nodes.JSXClosingFragment)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJsxClosingFragment(this);
        }
    }
}
