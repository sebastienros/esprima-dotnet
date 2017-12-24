namespace Esprima.Ast
{
    public class MetaProperty : Node, Expression
    {
        public readonly Identifier Meta;
        public readonly Identifier Property;

        public MetaProperty(Identifier meta, Identifier property)
        {
            Type = Nodes.MetaProperty;
            Meta = meta;
            Property = property;
        }
    }
}
