using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class PrivateIdentifier : Expression
{
    public PrivateIdentifier(string name) : base(Nodes.PrivateIdentifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitPrivateIdentifier(this);
}
