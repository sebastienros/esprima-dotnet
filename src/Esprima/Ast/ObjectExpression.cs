using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectExpression : Node, Expression
    {
        public List<Property> Properties { get; }

        public ObjectExpression(List<Property> properties)
        {
            Type = Nodes.ObjectExpression;
            Properties = properties;
        }
    }
}