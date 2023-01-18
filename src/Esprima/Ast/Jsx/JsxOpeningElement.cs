using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxOpeningElement : JsxExpression
{
    private readonly NodeList<JsxExpression> _attributes;

    public JsxOpeningElement(JsxExpression name, bool selfClosing, in NodeList<JsxExpression> attributes) : base(JsxNodeType.OpeningElement)
    {
        Name = name;
        SelfClosing = selfClosing;
        _attributes = attributes;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<JsxExpression> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _attributes; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Name, Attributes);

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxOpeningElement(this);

    public JsxOpeningElement UpdateWith(JsxExpression name, in NodeList<JsxExpression> attributes)
    {
        if (name == Name && NodeList.AreSame(attributes, Attributes))
        {
            return this;
        }

        return new JsxOpeningElement(name, SelfClosing, attributes);
    }
}
