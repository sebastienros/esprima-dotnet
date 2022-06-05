using System.Diagnostics;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Name,nq}")]
public sealed class JsxIdentifier : JsxExpression
{
    public readonly string Name;

    public JsxIdentifier(string name) : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxIdentifier(this);
    }
}
