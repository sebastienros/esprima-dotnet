using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : Node, BindingPattern
    {
        public List<Property> Properties { get; }

        public ObjectPattern(List<Property> properties)
        {
            Type = Nodes.ObjectPattern;
            Properties = properties;
        }
    }
}