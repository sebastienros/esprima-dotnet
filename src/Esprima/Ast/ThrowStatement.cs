﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ThrowStatement : Statement
    {
        public readonly Expression Argument;

        public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitThrowStatement(this);
        }
    }
}
