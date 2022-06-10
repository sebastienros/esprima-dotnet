using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxMemberExpression : JsxExpression
{
    public JsxMemberExpression(JsxExpression obj, JsxIdentifier property) : base(JsxNodeType.MemberExpression)
    {
        Object = obj;
        Property = property;
    }

    public JsxExpression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public JsxIdentifier Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override NodeCollection ChildNodes => new(Object, Property);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxMemberExpression(this);
    }

    public JsxMemberExpression UpdateWith(JsxExpression obj, JsxIdentifier property)
    {
        if (obj == Object && property == Property)
        {
            return this;
        }

        return new JsxMemberExpression(obj, property).SetAdditionalInfo(this);
    }
}
