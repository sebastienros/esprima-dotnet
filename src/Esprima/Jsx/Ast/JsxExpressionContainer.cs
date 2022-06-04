﻿using Esprima.Ast;
using Esprima.Jsx.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxExpressionContainer : JsxExpression
{
    public readonly Expression Expression;

    public JsxExpressionContainer(Expression expression) : base(JsxNodeType.ExpressionContainer)
    {
        Expression = expression;
    }

    public override NodeCollection ChildNodes => new(Expression);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxExpressionContainer(this);
    }

    public JsxExpressionContainer UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new JsxExpressionContainer(expression);
    }
}
