using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Name,nq}")]
public sealed class JsxIdentifier : JsxExpression
{
    public JsxIdentifier(string name) : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxIdentifier(this);
    }
}
