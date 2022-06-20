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

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Name, Attributes);

    protected override object? Accept(IJsxAstVisitor visitor, object? context)
    {
        return visitor.VisitJsxOpeningElement(this, context);
    }

    public JsxOpeningElement UpdateWith(JsxExpression name, in NodeList<JsxExpression> attributes)
    {
        if (name == Name && NodeList.AreSame(attributes, Attributes))
        {
            return this;
        }

        return new JsxOpeningElement(name, SelfClosing, attributes);
    }
}
