namespace Esprima.Ast
{
    public class TaggedTemplateExpression : Node, Expression
    {
        public Expression Tag { get; }
        public TemplateLiteral Quasi { get; }

        public TaggedTemplateExpression(Expression tag, TemplateLiteral quasi)
        {
            Type = Nodes.TaggedTemplateExpression;
            Tag = tag;
            Quasi = quasi;
        }
    }
}