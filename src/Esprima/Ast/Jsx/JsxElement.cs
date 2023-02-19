using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(OpeningElement), nameof(Children), nameof(ClosingElement) })]
public sealed partial class JsxElement : JsxExpression
{
    private readonly NodeList<JsxExpression> _children;

    public JsxElement(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement) : base(JsxNodeType.Element)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
        _children = children;
    }

    public Node OpeningElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Node? ClosingElement { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<JsxExpression> Children { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _children; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsxElement Rewrite(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement)
    {
        return new JsxElement(openingElement, children, closingElement);
    }
}
