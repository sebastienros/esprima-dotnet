using Esprima.Utils;

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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitObjectPattern(this);
        }

        public ObjectPattern UpdateWith(in NodeList<Node> properties)
        {
            if (NodeList.AreSame(properties, Properties))
            {
                return this;
            }

            return new ObjectPattern(properties);
        }
    }
}
