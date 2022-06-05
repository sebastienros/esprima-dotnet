using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxOpeningFragment : JsxExpression
{
    public readonly bool SelfClosing;

    public JsxOpeningFragment(bool selfClosing) : base(JsxNodeType.OpeningFragment)
    {
        SelfClosing = selfClosing;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxOpeningFragment(this);
    }
}
