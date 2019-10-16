using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        private NodeList<Property> _properties;

        public ObjectPattern(in NodeList<Property> properties) :
            base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Property> Properties => ref _properties;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_properties);
    }
}