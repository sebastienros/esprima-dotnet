using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Raw,nq}")]
public sealed class JsxText : JsxExpression
{
    public JsxText(StringSegment? value, StringSegment raw) : base(JsxNodeType.Text)
    {
        Value = value;
        Raw = raw;
    }

    public StringSegment? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public StringSegment Raw { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected override object? Accept(IJsxAstVisitor visitor) => visitor.VisitJsxText(this);
}
