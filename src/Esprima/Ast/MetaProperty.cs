using System.Collections.Generic;

namespace Esprima.Ast
{
    public class MetaProperty : Node, Expression
    {
        public readonly Identifier Meta;
        public readonly Identifier Property;

        public MetaProperty(Identifier meta, Identifier property) :
            base(Nodes.MetaProperty)
        {
            Meta = meta;
            Property = property;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Meta, Property);
    }
}
