using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast.Jsx;

[DebuggerDisplay("{Name,nq}")]
public sealed class JsxIdentifier : JsxExpression
{
    public JsxIdentifier(StringSegment name) : base(JsxNodeType.Identifier)
    {
        Name = name;
    }

    public StringSegment Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected override object? Accept(IJsxAstVisitor visitor) => visitor.VisitJsxIdentifier(this);
}
