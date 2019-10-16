using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        private readonly NodeList<IStatementListItem> _body;

        public BlockStatement(in NodeList<IStatementListItem> body) :
            base(Nodes.BlockStatement)
        {
            _body = body;
        }

        public ref readonly NodeList<IStatementListItem> Body => ref _body;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_body);
    }
}