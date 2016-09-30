namespace Esprima.Ast
{
    public class TaggedTemplateExpression : Node,
        Expression
    {
        public Expression Tag;
        public TemplateLiteral Quasi;
        public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi)
        {
            Type = Nodes.TaggedTemplateExpression;
            Tag = tag;
            Quasi = quasi;
        }
    }
}
