﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string? op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }

        internal UpdateExpression(UnaryOperator op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }

        protected override UnaryExpression Rewrite(Expression argument)
        {
            return new UpdateExpression(Operator, argument, Prefix);
        }
    }
}
