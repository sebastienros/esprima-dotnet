﻿using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class BlockStatement : Statement
    {
        private readonly NodeList<Statement> _body;

        public BlockStatement(in NodeList<Statement> body) : base(Nodes.BlockStatement)
        {
            _body = body;
        }

        public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

        public sealed override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);

        protected internal sealed override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitBlockStatement(this);
        }

        public BlockStatement UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new BlockStatement(body);
        }
    }
}
