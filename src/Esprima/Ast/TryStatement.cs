namespace Esprima.Ast
{
    public class TryStatement : Statement
    {
        public readonly Statement Block;
        public readonly CatchClause Handler;
        public readonly Statement Finalizer;

        public TryStatement(
            Statement block,
            CatchClause handler,
            Statement finalizer)
        {
            Type = Nodes.TryStatement;
            Block = block;
            Handler = handler;
            Finalizer = finalizer;
        }
    }
}