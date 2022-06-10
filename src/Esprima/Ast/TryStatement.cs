using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
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

        public override NodeCollection ChildNodes => new(Block, Handler, Finalizer);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitTryStatement(this);
        }

        public TryStatement UpdateWith(BlockStatement block, CatchClause? handler, BlockStatement? finalizer)
        {
            if (block == Block && handler == Handler && finalizer == Finalizer)
            {
                return this;
            }

            return new TryStatement(block, handler, finalizer);
        }
    }
}
