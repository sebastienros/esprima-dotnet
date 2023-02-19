using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Tag), nameof(Quasi) })]
public sealed partial class TaggedTemplateExpression : Expression
{
    public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi) : base(Nodes.TaggedTemplateExpression)
    {
        Tag = tag;
        Quasi = quasi;
    }

    public Expression Tag { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public TemplateLiteral Quasi { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TaggedTemplateExpression Rewrite(Expression tag, TemplateLiteral quasi)
    {
        return new TaggedTemplateExpression(tag, quasi);
    }
}
