using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source), nameof(Assertions) })]
public sealed partial class ExportAllDeclaration : ExportDeclaration
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExportAllDeclaration Rewrite(Expression? exported, Literal source, in NodeList<ImportAttribute> assertions)
    {
        return new ExportAllDeclaration(source, exported, assertions);
    }
}
