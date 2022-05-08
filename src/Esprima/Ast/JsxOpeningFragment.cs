﻿using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxOpeningFragment : JsxExpression
{
    public readonly bool SelfClosing;

    public JsxOpeningFragment(bool selfClosing) : base(Nodes.JSXOpeningFragment)
    {
        SelfClosing = selfClosing;
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxOpeningFragment(this) as T;
    }
}
