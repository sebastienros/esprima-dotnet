using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration), nameof(Specifiers), nameof(Source), nameof(Assertions) })]
public sealed partial class ExportNamedDeclaration : ExportDeclaration
{
    private readonly NodeList<ExportSpecifier> _specifiers;
    private readonly NodeList<ImportAttribute> _assertions;

    public ExportNamedDeclaration(
        Declaration? declaration,
        in NodeList<ExportSpecifier> specifiers,
        Literal? source,
        in NodeList<ImportAttribute> assertions)
        : base(Nodes.ExportNamedDeclaration)
    {
        Declaration = declaration;
        _specifiers = specifiers;
        Source = source;
        _assertions = assertions;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public Declaration? Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ExportSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal? Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _assertions; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExportNamedDeclaration Rewrite(Declaration? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> assertions)
    {
        return new ExportNamedDeclaration(declaration, specifiers, source, assertions);
    }
}
