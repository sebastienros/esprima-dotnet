namespace Esprima.Ast
{
    public class MetaProperty : Node, Expression
    {
        public Identifier Meta { get; }
        public Identifier Property { get; }

        public MetaProperty(Identifier meta, Identifier property)
        {
            Type = Nodes.MetaProperty;
            Meta = meta;
            Property = property;
        }
    }
}
