using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxText : JsxExpression
{
    public JsxText(string? value, string raw) : base(JsxNodeType.Text)
    {
        Value = value;
        Raw = raw;
    }

    public string? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
