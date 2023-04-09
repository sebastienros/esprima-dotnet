using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public abstract class ImportDeclarationSpecifier : Node, IModuleSpecifier
{
    protected ImportDeclarationSpecifier(Identifier local, Nodes type) : base(type)
    {
        Local = local;
    }

    public Identifier Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    Expression IModuleSpecifier.Local => Local;
}
