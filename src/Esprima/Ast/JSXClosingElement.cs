using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXClosingElement : JSXExpression
    {
        public readonly JSXExpression Name;

        public JSXClosingElement(JSXExpression name) : base(Nodes.JSXClosingElement)
        {
            Name = name;
        }
        public override NodeCollection ChildNodes => new(Name);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXClosingElement(this);
        }
    }
}
