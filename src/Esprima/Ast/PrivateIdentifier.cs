using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode]
public sealed partial class PrivateIdentifier : Expression
{
    public PrivateIdentifier(string name) : base(Nodes.PrivateIdentifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
