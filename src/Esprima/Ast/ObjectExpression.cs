using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectExpression : Node, Expression
    {
        public readonly List<Property> Properties;

        public ObjectExpression(List<Property> properties) :
            base(Nodes.ObjectExpression)
        {
            Properties = properties;
        }
    }
}