using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXSpreadAttribute : JSXExpression
    {
        public readonly Expression Argument;

        public JSXSpreadAttribute(Expression argument) : base(Nodes.JSXSpreadAttribute)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXSpreadAttribute(this);
        }
    }
}
