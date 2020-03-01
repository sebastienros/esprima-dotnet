﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression, IFunction
    {
        private readonly NodeList<INode> _params;

        public ArrowFunctionExpression(
            in NodeList<INode> parameters,
            INode body,
            bool expression,
            bool async,
            HoistingScope hoistingScope) :
            base(Nodes.ArrowFunctionExpression)
        {
            Id = null;
            _params = parameters;
            Body = body;
            Generator = false;
            Expression = expression;
            Async = async;
            HoistingScope = hoistingScope;
        }

        public Identifier Id { get; }
        public INode Body { get; } // : BlockStatement | Expression;
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Strict { get; }
        public bool Async { get; }
        public HoistingScope HoistingScope { get; }

        public ref readonly NodeList<INode> Params => ref _params;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Params, Body);        
    }
}
