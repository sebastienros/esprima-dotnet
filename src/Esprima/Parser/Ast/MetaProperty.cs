namespace Esprima.Ast
{
    public class MetaProperty : Node, Expression
    {
        public Identifier Meta;
        public Identifier Property;

        public MetaProperty(Identifier meta, Identifier property)
        {
            Type = Nodes.MetaProperty;
            Meta = meta;
            Property = property;
        }
    }
}
