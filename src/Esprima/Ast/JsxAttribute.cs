using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxAttribute : JsxExpression
{
    public readonly JsxExpression Name;
    public readonly Expression? Value;

    public JsxAttribute(JsxExpression name, Expression? value) : base(Nodes.JSXAttribute)
    {
        Name = name;
        Value = value;
    }

    public override NodeCollection ChildNodes => new(Name,Value);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxAttribute(this);
    }
}
