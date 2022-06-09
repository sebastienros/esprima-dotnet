using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxElement : JsxExpression
{
    public readonly Node OpeningElement;
    public readonly Node? ClosingElement;
    private readonly NodeList<JsxExpression> _children;

    public JsxElement(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement) : base(JsxNodeType.Element)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
        _children = children;
    }

    public ref readonly NodeList<JsxExpression> Children => ref _children;

    public override NodeCollection ChildNodes => ClosingElement is null ? GenericChildNodeYield.Yield(OpeningElement, _children) : GenericChildNodeYield.Yield(OpeningElement, _children, ClosingElement);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxElement(this);
    }

    public JsxElement UpdateWith(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement)
    {
        if (openingElement == OpeningElement && NodeList.AreSame(children, Children) && closingElement == ClosingElement)
        {
            return this;
        }

        return new JsxElement(openingElement, children, closingElement).SetAdditionalInfo(this);
    }
}
