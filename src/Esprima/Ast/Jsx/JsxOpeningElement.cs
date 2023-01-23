using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name), nameof(Attributes) })]
public sealed partial class JsxOpeningElement : JsxExpression
{
    private readonly NodeList<JsxExpression> _attributes;

    public JsxOpeningElement(JsxExpression name, bool selfClosing, in NodeList<JsxExpression> attributes) : base(JsxNodeType.OpeningElement)
    {
        Name = name;
        SelfClosing = selfClosing;
        _attributes = attributes;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<JsxExpression> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsxOpeningElement Rewrite(JsxExpression name, in NodeList<JsxExpression> attributes)
    {
        return new JsxOpeningElement(name, SelfClosing, attributes);
    }
}
