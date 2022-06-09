using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<Node> _body;

        public ClassBody(in NodeList<Node> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        /// <summary>
        /// <see cref="MethodDefinition" /> | <see cref="PropertyDefinition" /> | <see cref="StaticBlock" /> | <see cref="AccessorProperty" />
        /// </summary>
        public ref readonly NodeList<Node> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassBody(this);
        }

        public ClassBody UpdateWith(in NodeList<Node> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new ClassBody(body).SetAdditionalInfo(this);
        }
    }
}
