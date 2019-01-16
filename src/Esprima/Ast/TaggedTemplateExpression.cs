using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TaggedTemplateExpression : Node, Expression
    {
        public readonly Expression Tag;
        public readonly TemplateLiteral Quasi;

        public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi) :
            base(Nodes.TaggedTemplateExpression)
        {
            Tag = tag;
            Quasi = quasi;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Tag, Quasi);
    }
}