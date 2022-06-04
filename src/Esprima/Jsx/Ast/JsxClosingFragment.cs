using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxClosingFragment : JsxExpression
{
    public JsxClosingFragment() : base(JsxNodeType.ClosingFragment)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxClosingFragment(this);
    }
}
