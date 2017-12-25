using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression
    {
        public Identifier Id { get; }
        public List<INode> Params { get; }
        public INode Body { get; } // : BlockStatement | Expression;
        public bool Generator { get; }
        public bool Expression { get; }

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