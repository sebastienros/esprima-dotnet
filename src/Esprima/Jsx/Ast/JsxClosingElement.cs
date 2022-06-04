using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxClosingElement : JsxExpression
{
    public readonly JsxExpression Name;

    public JsxClosingElement(JsxExpression name) : base(JsxNodeType.ClosingElement)
    {
        Name = name;
    }

    public override NodeCollection ChildNodes => new(Name);

    protected internal override object? Accept(AstVisitor visitor)
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
