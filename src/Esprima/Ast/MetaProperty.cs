using System.Collections.Generic;

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

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Meta, Property);
    }
}
