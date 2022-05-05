﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class RestElement : Expression
    {
        // Identifier in esprima but not forced and
        // for instance ...i[0] is a SpreadElement
        // which is reinterpreted to RestElement with a ComputerMemberExpression

        public readonly Expression Argument; // BindingIdentifier | BindingPattern

        public RestElement(Expression argument) : base(Nodes.RestElement)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitRestElement(this) as T;
        }
    }
}
