using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        public readonly List<IStatementListItem> Body;

        public BlockStatement(List<IStatementListItem> body) :
            base(Nodes.BlockStatement)
        {
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}