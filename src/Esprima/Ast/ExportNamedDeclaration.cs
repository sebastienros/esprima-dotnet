using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration), nameof(Specifiers), nameof(Source), nameof(Assertions) })]
public sealed class ExportNamedDeclaration : ExportDeclaration
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0_2(Declaration, Specifiers, Source, Assertions);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitExportNamedDeclaration(this);

    public ExportNamedDeclaration UpdateWith(Declaration? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> assertions)
    {
        if (declaration == Declaration && NodeList.AreSame(specifiers, Specifiers) && source == Source && NodeList.AreSame(assertions, Assertions))
        {
            return this;
        }

        return new ExportNamedDeclaration(declaration, specifiers, source, assertions);
    }
}
