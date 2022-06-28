using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TaggedTemplateExpression : Expression
    {
        public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi) : base(Nodes.TaggedTemplateExpression)
        {
            Tag = tag;
            Quasi = quasi;
        }

        public Expression Tag { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public TemplateLiteral Quasi { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Tag, Quasi);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitTaggedTemplateExpression(this);

        public TaggedTemplateExpression UpdateWith(Expression tag, TemplateLiteral quasi)
        {
            if (tag == Tag && quasi == Quasi)
            {
                return this;
            }

            return new TaggedTemplateExpression(tag, quasi);
        }
    }
}
