using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        public readonly List<StatementListItem> Body;

        public BlockStatement(List<StatementListItem> body)
        {
            Type = Nodes.BlockStatement;
            Body = body;
        }
    }
}