using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxClosingFragment : JsxExpression
{

    public JsxClosingFragment() : base(Nodes.JSXClosingFragment)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxClosingFragment(this);
    }
}
