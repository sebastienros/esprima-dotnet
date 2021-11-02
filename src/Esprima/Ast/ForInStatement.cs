﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForInStatement : Statement
    {
        public readonly Node Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForInStatement(
            Node left,
            Expression right,
            Statement body) : base(Nodes.ForInStatement)
        {
            Left = left;
            Right = right;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Left, Right, Body);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitForInStatement(this);
        }
    }
}
