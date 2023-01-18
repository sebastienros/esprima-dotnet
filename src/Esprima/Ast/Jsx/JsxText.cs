using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Raw,nq}")]
public sealed class JsxText : JsxExpression
{
    public JsxText(string? value, string raw) : base(JsxNodeType.Text)
    {
        Value = value;
        Raw = raw;
    }

    public string? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public string Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxText(this);
}
