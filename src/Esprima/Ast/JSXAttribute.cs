using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXAttribute : JSXExpression
    {
        public readonly JSXExpression Name;
        public readonly Expression? Value;

        public JSXAttribute(JSXExpression name, Expression? value) : base(Nodes.JSXAttribute)
        {
            Name = name;
            Value = value;
        }

        public override NodeCollection ChildNodes => new(Name,Value);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXAttribute(this);
        }
    }
}
