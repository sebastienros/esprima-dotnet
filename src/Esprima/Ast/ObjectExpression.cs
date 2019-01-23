using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectExpression : Node, Expression
    {
        public readonly NodeList<Property> Properties;

        public ObjectExpression(NodeList<Property> properties) :
            base(Nodes.ObjectExpression)
        {
            Properties = properties;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Properties);
    }
}