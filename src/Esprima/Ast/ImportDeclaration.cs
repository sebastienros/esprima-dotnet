using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source), nameof(Assertions) })]
public sealed partial class ImportDeclaration : Declaration
{
    private readonly NodeList<ImportDeclarationSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _assertions;

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        Literal source,
        in NodeList<ImportAttribute> assertions)
        : base(Nodes.ImportDeclaration)
    {
        _specifiers = specifiers;
        Source = source;
        _assertions = assertions;
    }

    public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _assertions; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportDeclaration Rewrite(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source, in NodeList<ImportAttribute> assertions)
    {
        return new ImportDeclaration(specifiers, source, assertions);
    }
}
