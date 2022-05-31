using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxElement : JsxExpression
{
    public readonly Node OpeningElement;
    public readonly Node? ClosingElement;
    private readonly NodeList<JsxExpression> _children;

    public JsxElement(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement) : base(Nodes.JSXElement)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
        _children = children;
    }

    public ref readonly NodeList<JsxExpression> Children => ref _children;
    
    public override NodeCollection ChildNodes => ClosingElement is null ? GenericChildNodeYield.Yield(OpeningElement, _children) : GenericChildNodeYield.Yield(OpeningElement, _children, ClosingElement);

    protected internal override void Accept(AstVisitor visitor)
    {
        visitor.VisitJsxElement(this);
    }
}
