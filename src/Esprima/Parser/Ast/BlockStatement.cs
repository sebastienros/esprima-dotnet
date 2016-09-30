using System;
using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Node, Statement
    {
        public IEnumerable<StatementListItem> Body;

        public BlockStatement(IEnumerable<StatementListItem> body)
        {
            Type = Nodes.BlockStatement;
            Body = body;
        }
    }
}