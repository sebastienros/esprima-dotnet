using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression
    {
        public readonly Identifier Id;
        public readonly List<INode> Params;
        public readonly INode Body; // : BlockStatement | Expression;
        public readonly bool Generator;
        public readonly bool Expression;

        public ArrowFunctionExpression(List<INode> parameters, INode body, bool expression)
        {
            Type = Nodes.ArrowFunctionExpression;
            Id = null;
            Params = parameters;
            Body = body;
            Generator = false;
            Expression = expression;
        }
    }
}
