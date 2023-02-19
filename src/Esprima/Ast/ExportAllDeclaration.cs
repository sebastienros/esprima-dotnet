using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source) })]
public sealed partial class ExportAllDeclaration : ExportDeclaration
{
    public ExportAllDeclaration(Literal source) : this(source, null)
    {
    }

    public ExportAllDeclaration(Literal source, Expression? exported) : base(Nodes.ExportAllDeclaration)
    {
        Source = source;
        Exported = exported;
    }

    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExportAllDeclaration Rewrite(Expression? exported, Literal source)
    {
        return new ExportAllDeclaration(source, exported);
    }
}
