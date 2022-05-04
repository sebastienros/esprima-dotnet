using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXClosingFragment : JSXExpression
    {

        public JSXClosingFragment() : base(Nodes.JSXClosingFragment)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXClosingFragment(this);
        }
    }
}
