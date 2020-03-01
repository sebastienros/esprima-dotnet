using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        private readonly NodeList<ObjectPatternProperty> _properties;

        public ObjectPattern(in NodeList<ObjectPatternProperty> properties) :
            base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        public ref readonly NodeList<ObjectPatternProperty> Properties => ref _properties;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_properties);
    }
}