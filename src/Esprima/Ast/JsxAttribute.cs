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

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxAttribute(this) as T;
    }
}
