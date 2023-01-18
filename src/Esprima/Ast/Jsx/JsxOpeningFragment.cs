using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxOpeningFragment : JsxExpression
{
    public JsxOpeningFragment(bool selfClosing) : base(JsxNodeType.OpeningFragment)
    {
        SelfClosing = selfClosing;
    }

    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxOpeningFragment(this);
}
