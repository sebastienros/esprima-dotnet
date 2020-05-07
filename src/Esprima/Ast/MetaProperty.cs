namespace Esprima.Ast
{
    public sealed class MetaProperty : Expression
    {
        public readonly Identifier Meta;
        public readonly Identifier Property;

        public MetaProperty(Identifier meta, Identifier property) : base(Nodes.MetaProperty)
        {
            Meta = meta;
            Property = property;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Meta, Property);
    }
}
