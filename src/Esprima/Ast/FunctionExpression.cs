using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class FunctionExpression : Node,
        Expression,
        PropertyValue
    {
        public Identifier Id { get; set; }
        public IEnumerable<INode> Params { get; set; }
        public BlockStatement Body { get; set; }
        [JsonIgnore]
        public bool Strict { get; set; }

        public bool Generator;
        public bool Expression;

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