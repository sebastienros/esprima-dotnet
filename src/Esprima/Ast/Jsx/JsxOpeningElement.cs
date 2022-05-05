using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxOpeningElement : JsxExpression
{
    internal readonly NodeList<JsxExpression> _attributes;

    public JsxOpeningElement(JsxExpression name, bool selfClosing, in NodeList<JsxExpression> attributes) : base(JsxNodeType.OpeningElement)
    {
        Name = name;
        SelfClosing = selfClosing;
        _attributes = attributes;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool SelfClosing { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ReadOnlySpan<JsxExpression> Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _attributes.AsSpan(); }

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Name, _attributes);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxOpeningElement(this);
    }

    public JsxOpeningElement UpdateWith(JsxExpression name, in NodeList<JsxExpression> attributes)
    {
        if (name == Name && NodeList.AreSame(attributes, _attributes))
        {
            return this;
        }

        return new JsxOpeningElement(name, SelfClosing, attributes);
    }
}
