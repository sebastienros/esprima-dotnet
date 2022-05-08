using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxClosingFragment : JsxExpression
{

    public JsxClosingFragment() : base(Nodes.JSXClosingFragment)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxClosingFragment(this) as T;
    }
}
