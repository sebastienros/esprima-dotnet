using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxOpeningElement : JsxExpression
{
    public readonly JsxExpression Name;
    public readonly bool SelfClosing;
    private readonly NodeList<JsxExpression> _attributes;

    public JsxOpeningElement(JsxExpression name, bool selfClosing, in NodeList<JsxExpression> attributes) : base(Nodes.JSXOpeningElement)
    {
        Name = name;
        SelfClosing = selfClosing;
        _attributes = attributes;
    }

    public ref readonly NodeList<JsxExpression> Attributes => ref _attributes;
    
    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Name, _attributes);

    protected internal override void Accept(AstVisitor visitor)
    {
        visitor.VisitJsxOpeningElement(this);
    }
}
