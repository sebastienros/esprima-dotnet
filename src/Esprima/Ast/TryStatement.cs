﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class TryStatement : Statement
    {
        public readonly BlockStatement Block;
        public readonly CatchClause? Handler;
        public readonly BlockStatement? Finalizer;

        public TryStatement(
            BlockStatement block,
            CatchClause? handler,
            BlockStatement? finalizer) :
            base(Nodes.TryStatement)
        {
            Block = block;
            Handler = handler;
            Finalizer = finalizer;
        }

        public override NodeCollection ChildNodes => new(Block, Handler, Finalizer);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitTryStatement(this);
        }
    }
}
