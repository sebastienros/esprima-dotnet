using System.Runtime.CompilerServices;
using Esprima.Utils;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class Identifier : Expression
{
    public Identifier(StringSegment name) : base(Nodes.Identifier)
    {
        Name = name;
    }

    public StringSegment Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitIdentifier(this);
}
