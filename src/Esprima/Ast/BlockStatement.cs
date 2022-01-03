using Esprima.Utils;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        private readonly NodeList<Statement> _body;

        public BlockStatement(in NodeList<Statement> body) : base(Nodes.BlockStatement)
        {
            _body = body;
        }

        internal BlockStatement(in NodeList<Statement> body, Nodes type) : base(type)
        {
            _body = body;
        }

        public ref readonly NodeList<Statement> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitBlockStatement(this);
        }
    }
}
