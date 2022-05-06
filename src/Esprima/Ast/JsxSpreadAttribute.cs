using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JsxSpreadAttribute : JsxExpression
    {
        public readonly Expression Argument;

        public JsxSpreadAttribute(Expression argument) : base(Nodes.JSXSpreadAttribute)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJsxSpreadAttribute(this);
        }
    }
}
