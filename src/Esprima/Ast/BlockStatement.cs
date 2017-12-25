using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        public List<StatementListItem> Body { get; }

        public BlockStatement(List<StatementListItem> body)
        {
            Type = Nodes.BlockStatement;
            Body = body;
        }
    }
}