using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxMemberExpression : JsxExpression
{
    public readonly JsxExpression Object;
    public readonly JsxIdentifier Property;

    public JsxMemberExpression(JsxExpression @object, JsxIdentifier property) : base(Nodes.JSXMemberExpression)
    {
        Object = @object;
        Property = property;
    }

    public override NodeCollection ChildNodes => new(Object, Property);
        
    protected internal override void Accept(AstVisitor visitor)
    {
        visitor.VisitJsxMemberExpression(this);
    }
}