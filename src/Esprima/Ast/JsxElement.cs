﻿using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxElement : JsxExpression
{
    public readonly Node OpeningElement;
    public readonly Node? ClosingElement;
    private readonly NodeList<JsxExpression> _children;

    public JsxElement(Node openingElement, in NodeList<JsxExpression> children, Node? closingElement) : base(Nodes.JSXElement)
    {
        OpeningElement = openingElement;
        ClosingElement = closingElement;
        _children = children;
    }

    public ref readonly NodeList<JsxExpression> Children => ref _children;
    
    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(OpeningElement, _children, ClosingElement);

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxElement(this) as T;
    }
}
