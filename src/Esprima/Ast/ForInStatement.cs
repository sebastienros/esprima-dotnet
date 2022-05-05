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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitForInStatement(this) as T;
        }
    }
}
