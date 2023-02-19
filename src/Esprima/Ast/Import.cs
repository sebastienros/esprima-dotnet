using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source), nameof(Attributes) })]
public sealed partial class Import : Expression
{
    public Import(Expression source) : this(source, null)
    {
    }

    public Import(Expression source, Expression? attributes) : base(Nodes.Import)
    {
        Source = source;
        Attributes = attributes;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Import Rewrite(Expression source, Expression? attributes)
    {
        return new Import(source, attributes);
    }
}
