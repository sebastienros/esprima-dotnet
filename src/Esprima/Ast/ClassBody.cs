using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<Expression> _body;

        public ClassBody(in NodeList<Expression> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        public ref readonly NodeList<Expression> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitClassBody(this);
        }
    }
}
