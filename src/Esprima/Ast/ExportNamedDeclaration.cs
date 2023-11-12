using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration), nameof(Specifiers), nameof(Source), nameof(Attributes) })]
public sealed partial class ExportNamedDeclaration : ExportDeclaration
{
    private readonly NodeList<ExportSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _attributes;

    public ExportNamedDeclaration(
        Declaration? declaration,
        in NodeList<ExportSpecifier> specifiers,
        Literal? source,
        in NodeList<ImportAttribute> attributes)
        : base(Nodes.ExportNamedDeclaration)
    {
        Declaration = declaration;
        _specifiers = specifiers;
        Source = source;
        _attributes = attributes;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public Declaration? Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ExportSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal? Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExportNamedDeclaration Rewrite(Declaration? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> attributes)
    {
        return new ExportNamedDeclaration(declaration, specifiers, source, attributes);
    }
}
