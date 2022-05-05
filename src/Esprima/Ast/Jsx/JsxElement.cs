using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxElement : JsxExpression
{
    internal readonly NodeList<JsxExpression> _children;

    public JsxElement(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement) : base(JsxNodeType.Element)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
        _children = children;
    }

    public Node OpeningElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Node? ClosingElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ReadOnlySpan<JsxExpression> Children { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _children.AsSpan(); }

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(OpeningElement, _children, ClosingElement);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxElement(this);
    }

    public JsxElement UpdateWith(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement)
    {
        if (openingElement == OpeningElement && NodeList.AreSame(children, _children) && closingElement == ClosingElement)
        {
            return this;
        }

        return new JsxElement(openingElement, children, closingElement);
    }
}
