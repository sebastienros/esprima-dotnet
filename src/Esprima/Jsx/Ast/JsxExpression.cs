﻿using Esprima.Ast;
using Esprima.Jsx.Utils;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

/// <summary>
/// A Jsx expression.
/// </summary>
public abstract class JsxExpression : Expression
{
    public new readonly JsxNodeType Type;

    protected JsxExpression(JsxNodeType type) : base(Nodes.Extension)
    {
        Type = type;
    }

    protected abstract object? Accept(IJsxAstVisitor visitor);

    protected internal sealed override object? Accept(AstVisitor visitor)
    {
        return visitor is IJsxAstVisitor jsxVisitor ? Accept(jsxVisitor) : visitor.VisitExtension(this);
    }
}
