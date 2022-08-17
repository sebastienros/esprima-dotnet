using System.Runtime.CompilerServices;
using Esprima.Utils;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class PrivateIdentifier : Expression
{
    public PrivateIdentifier(StringSegment name) : base(Nodes.PrivateIdentifier)
    {
        Name = name;
    }

    public StringSegment Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitPrivateIdentifier(this);
}
