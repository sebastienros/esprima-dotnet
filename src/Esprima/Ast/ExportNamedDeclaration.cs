using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration), nameof(Specifiers), nameof(Source) })]
public sealed partial class ExportNamedDeclaration : ExportDeclaration
{
    private readonly NodeList<ExportSpecifier> _specifiers;

    public ExportNamedDeclaration(
        Declaration? declaration,
        in NodeList<ExportSpecifier> specifiers,
        Literal? source)
        : base(Nodes.ExportNamedDeclaration)
    {
        Declaration = declaration;
        _specifiers = specifiers;
        Source = source;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public Declaration? Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ExportSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
    public Literal? Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExportNamedDeclaration Rewrite(Declaration? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source)
    {
        return new ExportNamedDeclaration(declaration, specifiers, source);
    }
}
