using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay("{Raw,nq}")]
public sealed class JsxText : JsxExpression
{
    public readonly string? Value;
    public readonly string Raw;

    public JsxText(string? value, string raw) : base(Nodes.JSXText)
    {
        Value = value;
        Raw = raw;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxText(this);
    }
}
