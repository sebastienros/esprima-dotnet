using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxClosingElement : JsxExpression
{
    public readonly JsxExpression Name;

    public JsxClosingElement(JsxExpression name) : base(JsxNodeType.ClosingElement)
    {
        Name = name;
    }

    public override NodeCollection ChildNodes => new(Name);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxClosingElement(this);
    }

    public JsxClosingElement UpdateWith(JsxExpression name)
    {
        if (name == Name)
        {
            return this;
        }

        return new JsxClosingElement(name);
    }
}
