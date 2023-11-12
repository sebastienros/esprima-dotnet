using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Block), nameof(Handler), nameof(Finalizer) })]
public sealed partial class TryStatement : Statement
{
    public TryStatement(
        BlockStatement block,
        CatchClause? handler,
        BlockStatement? finalizer) :
        base(Nodes.TryStatement)
    {
        Block = block;
        Handler = handler;
        Finalizer = finalizer;
    }

    public BlockStatement Block { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public CatchClause? Handler { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public BlockStatement? Finalizer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TryStatement Rewrite(BlockStatement block, CatchClause? handler, BlockStatement? finalizer)
    {
        return new TryStatement(block, handler, finalizer);
    }
}
