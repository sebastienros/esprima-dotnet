using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class TaggedTemplateExpression : Expression
    {
        public readonly Expression Tag;
        public readonly TemplateLiteral Quasi;

        public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi) : base(Nodes.TaggedTemplateExpression)
        {
            Tag = tag;
            Quasi = quasi;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Tag, Quasi);
    }
}