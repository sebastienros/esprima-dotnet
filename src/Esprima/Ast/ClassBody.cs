﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<Node> _body;

        public ClassBody(in NodeList<Node> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        public ref readonly NodeList<Node> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitClassBody(this) as T;
        }
    }
}
