using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxMemberExpression : JsxExpression
{
    public readonly JsxExpression Object;
    public readonly JsxIdentifier Property;

    public JsxMemberExpression(JsxExpression obj, JsxIdentifier property) : base(Nodes.JSXMemberExpression)
    {
        Object = obj;
        Property = property;
    }

    public override NodeCollection ChildNodes => new(Object, Property);
        
    protected internal override Node Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxMemberExpression(this);
    }
}
