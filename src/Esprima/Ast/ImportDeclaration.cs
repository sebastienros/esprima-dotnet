using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source) })]
public sealed partial class ImportDeclaration : Declaration
{
    private readonly NodeList<ImportDeclarationSpecifier> _specifiers;

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        Literal source)
        : base(Nodes.ImportDeclaration)
    {
        _specifiers = specifiers;
        Source = source;
    }

    public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportDeclaration Rewrite(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source)
    {
        return new ImportDeclaration(specifiers, source);
    }
}
