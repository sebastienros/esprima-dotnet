using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXOpeningElement : JSXExpression
    {
        public readonly JSXExpression Name;
        public readonly bool SelfClosing;
        private readonly NodeList<JSXExpression> _attributes;

        public JSXOpeningElement(JSXExpression name, bool selfClosing, in NodeList<JSXExpression> attributes) : base(Nodes.JSXOpeningElement)
        {
            Name = name;
            SelfClosing = selfClosing;
            _attributes = attributes;
        }

        public ref readonly NodeList<JSXExpression> Attributes => ref _attributes;
        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Name,_attributes);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXOpeningElement(this);
        }
    }
}
