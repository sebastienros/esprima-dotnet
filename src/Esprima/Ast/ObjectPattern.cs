using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        public IEnumerable<Property> Properties;
        public ObjectPattern(IEnumerable<Property> properties)
        {
            Type = Nodes.ObjectPattern;
            Properties = properties;
        }
    }
}
