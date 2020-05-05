using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        private readonly NodeList<Statement> _body;

        public BlockStatement(in NodeList<Statement> body) :
            base(Nodes.BlockStatement)
        {
            _body = body;
        }

        public ref readonly NodeList<Statement> Body => ref _body;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(_body);
    }
}