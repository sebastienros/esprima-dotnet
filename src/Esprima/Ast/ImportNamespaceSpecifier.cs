using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Local) })]
public sealed partial class ImportNamespaceSpecifier : ImportDeclarationSpecifier
{
    public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImportNamespaceSpecifier Rewrite(Identifier local)
    {
        return new ImportNamespaceSpecifier(local);
    }
}
