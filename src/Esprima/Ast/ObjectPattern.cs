using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        public readonly List<Property> Properties;

        public ObjectPattern(List<Property> properties) :
            base(Nodes.ObjectPattern)
        {
            Properties = properties;
        }
    }
}