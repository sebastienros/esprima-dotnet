using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class JSXMemberExpression : JSXExpression
    {
        public readonly JSXExpression Object;
        public readonly JSXIdentifier Property;

        public JSXMemberExpression(JSXExpression @object, JSXIdentifier property) : base(Nodes.JSXMemberExpression)
        {
            Object = @object;
            Property = property;
        }

        public override NodeCollection ChildNodes => new(Object, Property);
        
        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXMemberExpression(this);
        }
    }
}
