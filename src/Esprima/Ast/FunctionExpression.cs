using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionExpression : Node,
        IFunction,
        Expression,
        PropertyValue
    {
        public Identifier Id { get; }
        public NodeList<INode> Params { get; }
        public INode Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }

        public HoistingScope HoistingScope { get; }
        public bool Strict { get; }

        public FunctionExpression(
            Identifier id,
            NodeList<INode> parameters,
            BlockStatement body,
            bool generator,
            HoistingScope hoistingScope,
            bool strict) :
            base(Nodes.FunctionExpression)
        {
            Id = id;
            Params = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            HoistingScope = hoistingScope;
            Strict = strict;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, Params, Body);
    }
}