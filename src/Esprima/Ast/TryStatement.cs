namespace Esprima.Ast
{
    public class TryStatement : Statement
    {
        public Statement Block { get; }
        public CatchClause Handler { get; }
        public Statement Finalizer { get; }

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