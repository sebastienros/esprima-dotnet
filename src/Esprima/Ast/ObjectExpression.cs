using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectExpression : Node,
        Expression
    {
        public IEnumerable<Property> Properties;

        public ObjectExpression(IEnumerable<Property> properties)
        {
            Type = Nodes.ObjectExpression;
            Properties = properties;
        }
    }
}