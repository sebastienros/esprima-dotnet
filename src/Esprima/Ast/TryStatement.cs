using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TryStatement : Statement
    {
        public Statement Block;
        public CatchClause Handler;
        public Statement Finalizer;

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