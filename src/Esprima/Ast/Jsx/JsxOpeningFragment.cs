using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed class JsxOpeningFragment : JsxExpression
{
    public JsxOpeningFragment(bool selfClosing) : base(JsxNodeType.OpeningFragment)
    {
        SelfClosing = selfClosing;
    }

    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected override object? Accept(IJsxAstVisitor visitor) => visitor.VisitJsxOpeningFragment(this);
}
