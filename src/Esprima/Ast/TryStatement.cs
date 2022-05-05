using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TryStatement : Statement
    {
        public readonly Statement Block;
        public readonly CatchClause? Handler;
        public readonly Statement? Finalizer;

        public TryStatement(
            Statement block,
            CatchClause? handler,
            Statement? finalizer) :
            base(Nodes.TryStatement)
        {
            Block = block;
            Handler = handler;
            Finalizer = finalizer;
        }

        public override NodeCollection ChildNodes => new(Block, Handler, Finalizer);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitTryStatement(this) as T;
        }
    }
}
