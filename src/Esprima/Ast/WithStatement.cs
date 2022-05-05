﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class WithStatement : Statement
    {
        public readonly Expression Object;
        public readonly Statement Body;

        public WithStatement(Expression obj, Statement body) : base(Nodes.WithStatement)
        {
            Object = obj;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Object, Body);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitWithStatement(this) as T;
        }
    }
}
