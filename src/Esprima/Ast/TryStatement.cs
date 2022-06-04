using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TryStatement : Statement
    {
        public readonly BlockStatement Block;
        public readonly CatchClause? Handler;
        public readonly BlockStatement? Finalizer;

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
