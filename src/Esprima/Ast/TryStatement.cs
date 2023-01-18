using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class TryStatement : Statement
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1_2(Block, Handler, Finalizer);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitTryStatement(this);

    public TryStatement UpdateWith(BlockStatement block, CatchClause? handler, BlockStatement? finalizer)
    {
        if (block == Block && handler == Handler && finalizer == Finalizer)
        {
            return this;
        }

        return new TryStatement(block, handler, finalizer);
    }
}
