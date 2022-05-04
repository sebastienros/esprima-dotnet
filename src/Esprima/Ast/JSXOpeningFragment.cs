using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXOpeningFragment : JSXExpression
    {
        public readonly bool SelfClosing;

        public JSXOpeningFragment(bool selfClosing) : base(Nodes.JSXOpeningFragment)
        {
            SelfClosing = selfClosing;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXOpeningFragment(this);
        }
    }
}
