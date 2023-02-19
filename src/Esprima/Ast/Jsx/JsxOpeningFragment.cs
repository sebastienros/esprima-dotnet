using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxOpeningFragment : JsxExpression
{
    public JsxOpeningFragment(bool selfClosing) : base(JsxNodeType.OpeningFragment)
    {
        SelfClosing = selfClosing;
    }

    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
