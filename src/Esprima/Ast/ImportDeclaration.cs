using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source), nameof(Attributes) })]
public sealed partial class ImportDeclaration : Declaration
{
    private readonly NodeList<ImportDeclarationSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _attributes;

    public ImportDeclaration(
        in NodeList<ImportDeclarationSpecifier> specifiers,
        Literal source,
        in NodeList<ImportAttribute> attributes)
        : base(Nodes.ImportDeclaration)
    {
        _specifiers = specifiers;
        Source = source;
        _attributes = attributes;
    }

    public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ImportDeclaration Rewrite(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source, in NodeList<ImportAttribute> attributes)
    {
        return new ImportDeclaration(specifiers, source, attributes);
    }
}
