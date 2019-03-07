using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        public readonly NodeList<Property> Properties;

        public ObjectPattern(NodeList<Property> properties) :
            base(Nodes.ObjectPattern)
        {
            Properties = properties;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Properties);
    }
}