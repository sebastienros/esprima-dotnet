using Esprima.Utils;

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

        public override NodeCollection ChildNodes => new(Meta, Property);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitMetaProperty(this) as T;
        }
    }
}
