﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class AssignmentPattern : Expression
    {
        public readonly Expression Left;
        public Expression Right;

        public AssignmentPattern(Expression left, Expression right) : base(Nodes.AssignmentPattern)
        {
            Left = left;
            Right = right;
        }

        public override NodeCollection ChildNodes => new(Left, Right);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitAssignmentPattern(this);
        }
    }
}
