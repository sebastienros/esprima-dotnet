using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression
    {
        public Identifier Id;
        public IEnumerable<INode> Params;
        public INode Body; // : BlockStatement | Expression;
        public bool Generator;
        public bool Expression;

        public ArrowFunctionExpression(IEnumerable<INode> parameters, INode body, bool expression)
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
