﻿using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxOpeningElement : JsxExpression
{
    public readonly JsxExpression Name;
    public readonly bool SelfClosing;
    private readonly NodeList<JsxExpression> _attributes;

    public JsxOpeningElement(JsxExpression name, bool selfClosing, in NodeList<JsxExpression> attributes) : base(JsxNodeType.OpeningElement)
    {
        Name = name;
        SelfClosing = selfClosing;
        _attributes = attributes;
    }

    public ref readonly NodeList<JsxExpression> Attributes => ref _attributes;

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Name, _attributes);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxOpeningElement(this);
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
