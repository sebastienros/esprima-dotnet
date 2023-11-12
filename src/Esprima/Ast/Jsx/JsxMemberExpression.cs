using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Object), nameof(Property) })]
public sealed partial class JsxMemberExpression : JsxExpression
{
    public JsxMemberExpression(JsxExpression obj, JsxIdentifier property) : base(JsxNodeType.MemberExpression)
    {
        Object = obj;
        Property = property;
    }

    public JsxExpression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxIdentifier Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JsxMemberExpression Rewrite(JsxExpression @object, JsxIdentifier property)
    {
        return new JsxMemberExpression(@object, property);
    }
}
