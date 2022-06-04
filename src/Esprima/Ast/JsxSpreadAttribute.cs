﻿using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxSpreadAttribute : JsxExpression
{
    public readonly Expression Argument;

    public JsxSpreadAttribute(Expression argument) : base(Nodes.JSXSpreadAttribute)
    {
        Argument = argument;
    }

    public override NodeCollection ChildNodes => new(Argument);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxSpreadAttribute(this);
    }

    public JsxSpreadAttribute UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new JsxSpreadAttribute(argument);
    }
}
