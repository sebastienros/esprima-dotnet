﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class MethodDefinition : ClassProperty
    {
        public readonly bool Static;

        public MethodDefinition(
            Expression key,
            bool computed,
            FunctionExpression value,
            PropertyKind kind,
            bool isStatic)
            : base(Nodes.MethodDefinition)
        {
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitMethodDefinition(this);
        }

        public MethodDefinition UpdateWith(Expression key, FunctionExpression value)
        {
            if (key == Key && value == Value)
            {
                return this;
            }

            return new MethodDefinition(key, Computed, value, Kind, Static);
        }
    }
}
