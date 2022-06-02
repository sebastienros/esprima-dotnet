using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxClosingElement : JsxExpression
{
    public readonly JsxExpression Name;

    public JsxClosingElement(JsxExpression name) : base(Nodes.JSXClosingElement)
    {
        Name = name;
    }
    
    public override NodeCollection ChildNodes => new(Name);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxClosingElement(this);
    }
}
