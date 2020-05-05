using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class ObjectExpression : Expression
    {
        private readonly NodeList<Expression> _properties;

        public ObjectExpression(in NodeList<Expression> properties) : base(Nodes.ObjectExpression)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Expression> Properties => ref _properties;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(_properties);
    }
}