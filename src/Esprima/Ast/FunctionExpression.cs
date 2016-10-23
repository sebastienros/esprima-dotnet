using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionExpression : Node,
        IFunction,
        Expression,
        PropertyValue
    {
        public Identifier Id { get; set; }
        public IEnumerable<INode> Params { get; set; }
        public BlockStatement Body { get; set; }
        public bool Generator { get; set; }
        public bool Expression { get; set; }

        public FunctionExpression(
            Identifier id,
            IEnumerable<INode> parameters,
            BlockStatement body,
            bool generator
            )
        {
            Type = Nodes.FunctionExpression;
            Id = id;
            Params = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
        }
    }
}