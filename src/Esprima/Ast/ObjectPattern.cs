﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ObjectPattern : BindingPattern
    {
        private readonly NodeList<Node> _properties;

        public ObjectPattern(in NodeList<Node> properties) : base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Node> Properties => ref _properties;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_properties);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitObjectPattern(this) as T;
        }
    }
}
