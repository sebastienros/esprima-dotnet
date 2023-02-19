using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode]
public sealed partial class Identifier : Expression
{
    public Identifier(string name) : base(Nodes.Identifier)
    {
        Name = name;
    }

    public string Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
