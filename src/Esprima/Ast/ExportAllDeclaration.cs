using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Exported), nameof(Source), nameof(Attributes) })]
public sealed partial class ExportAllDeclaration : ExportDeclaration
{
    private readonly NodeList<ImportAttribute> _attributes;

    public ExportAllDeclaration(Literal source) : this(source, null, new NodeList<ImportAttribute>())
    {
    }

    public ExportAllDeclaration(Literal source, Expression? exported, in NodeList<ImportAttribute> attributes) : base(Nodes.ExportAllDeclaration)
    {
        Source = source;
        Exported = exported;
        _attributes = attributes;
    }

    public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<ImportAttribute> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExportAllDeclaration Rewrite(Expression? exported, Literal source, in NodeList<ImportAttribute> attributes)
    {
        return new ExportAllDeclaration(source, exported, attributes);
    }
}
