using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public abstract class ImportDeclarationSpecifier : Node
{
    protected ImportDeclarationSpecifier(Identifier local, Nodes type) : base(type)
    {
        Local = local;
    }

    public Identifier Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
