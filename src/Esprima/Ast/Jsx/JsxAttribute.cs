using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Name), nameof(Value) })]
public sealed partial class JsxAttribute : JsxExpression
{
    public JsxAttribute(JsxExpression name, Expression? value) : base(JsxNodeType.Attribute)
    {
        Name = name;
        Value = value;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsxAttribute Rewrite(JsxExpression name, Expression? value)
    {
        return new JsxAttribute(name, value);
    }
}
