using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name) })]
public sealed partial class JsxClosingElement : JsxExpression
{
    public JsxClosingElement(JsxExpression name) : base(JsxNodeType.ClosingElement)
    {
        Name = name;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsxClosingElement Rewrite(JsxExpression name)
    {
        return new JsxClosingElement(name);
    }
}
