using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Local) })]
public sealed partial class ImportDefaultSpecifier : ImportDeclarationSpecifier
{
    public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImportDefaultSpecifier Rewrite(Identifier local)
    {
        return new ImportDefaultSpecifier(local);
    }
}
