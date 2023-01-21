using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source), nameof(Assertions) })]
public sealed class ExportAllDeclaration : ExportDeclaration
{
    private readonly NodeList<ImportAttribute> _assertions;

    public ExportAllDeclaration(Literal source) : this(source, null, new NodeList<ImportAttribute>())
    {
    }

    public ExportAllDeclaration(Literal source, Expression? exported, in NodeList<ImportAttribute> assertions) : base(Nodes.ExportAllDeclaration)
    {
        Source = source;
        Exported = exported;
        _assertions = assertions;
    }

    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _assertions; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Exported, Source, Assertions);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitExportAllDeclaration(this);

    public ExportAllDeclaration UpdateWith(Expression? exported, Literal source, in NodeList<ImportAttribute> assertions)
    {
        if (exported == Exported && source == Source && NodeList.AreSame(assertions, Assertions))
        {
            return this;
        }

        return new ExportAllDeclaration(source, exported, assertions);
    }
}
