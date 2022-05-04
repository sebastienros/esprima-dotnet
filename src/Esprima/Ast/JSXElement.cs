using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXElement : JSXExpression
    {
        public readonly Node OpeningElement;
        public readonly Node? ClosingElement;
        private readonly NodeList<JSXExpression> _children;

        public JSXElement(Node openingElement, in NodeList<JSXExpression> children, Node? closingElement) : base(Nodes.JSXElement)
        {
            OpeningElement = openingElement;
            ClosingElement = closingElement;
            _children = children;
        }

        public ref readonly NodeList<JSXExpression> Children => ref _children;
        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(OpeningElement,_children,ClosingElement);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXElement(this);
        }
    }
}
