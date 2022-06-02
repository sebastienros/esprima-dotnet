﻿using Esprima.Utils;

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

        public override NodeCollection ChildNodes => new(Tag, Quasi);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitTaggedTemplateExpression(this);
        }
    }
}
