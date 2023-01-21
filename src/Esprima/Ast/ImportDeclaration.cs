using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Specifiers), nameof(Source), nameof(Assertions) })]
public sealed class ImportDeclaration : Declaration
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Specifiers, Source, Assertions);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImportDeclaration(this);

    public ImportDeclaration UpdateWith(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source, in NodeList<ImportAttribute> assertions)
    {
        if (NodeList.AreSame(specifiers, Specifiers) && source == Source && NodeList.AreSame(assertions, Assertions))
        {
            return this;
        }

        return new ImportDeclaration(specifiers, source, assertions);
    }
}
