using System.Diagnostics;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Raw,nq}")]
public sealed class JsxText : JsxExpression
{
    public readonly string? Value;
    public readonly string Raw;

    public JsxText(string? value, string raw) : base(JsxNodeType.Text)
    {
        Value = value;
        Raw = raw;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxText(this);
    }
}
