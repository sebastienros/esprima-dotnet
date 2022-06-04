using System.Diagnostics;
using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

[DebuggerDisplay("{Name,nq}")]
public sealed class JsxIdentifier : JsxExpression
{
    public readonly string Name;

    public JsxIdentifier(string name) : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxIdentifier(this);
    }
}
