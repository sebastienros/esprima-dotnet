using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxMemberExpression : JsxExpression
{
    public readonly JsxExpression Object;
    public readonly JsxIdentifier Property;

    public JsxMemberExpression(JsxExpression obj, JsxIdentifier property) : base(JsxNodeType.MemberExpression)
    {
        Object = obj;
        Property = property;
    }

    public override NodeCollection ChildNodes => new(Object, Property);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxMemberExpression(this);
    }

    public JsxMemberExpression UpdateWith(JsxExpression obj, JsxIdentifier property)
    {
        if (obj == Object && property == Property)
        {
            return this;
        }

        return new JsxMemberExpression(obj, property);
    }
}
