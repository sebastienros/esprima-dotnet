using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source) })]
public sealed partial class Import : Expression
{
    public Import(Expression source) : base(Nodes.Import)
    {
        Source = source;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Import Rewrite(Expression source)
    {
        return new Import(source);
    }
}
