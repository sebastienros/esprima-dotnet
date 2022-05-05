using Esprima.Utils;

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

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_properties);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitObjectExpression(this) as T;
        }
    }
}
