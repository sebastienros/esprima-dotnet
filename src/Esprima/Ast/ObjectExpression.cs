using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectExpression : Node, Expression
    {
        private readonly NodeList<Property> _properties;

        public ObjectExpression(in NodeList<Property> properties) :
            base(Nodes.ObjectExpression)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Property> Properties => ref _properties;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_properties);
    }
}