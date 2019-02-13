using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression, IFunction
    {
        public bool Strict { get; }

        public Identifier Id { get; }

        public List<INode> Params { get; }

        public INode Body { get; } // : BlockStatement | Expression;

        public bool Generator { get; }

        public bool Expression { get; }

        public HoistingScope HoistingScope { get; }

        public ArrowFunctionExpression(List<INode> parameters, INode body, bool expression, HoistingScope hoistingScope) :
            base(Nodes.ArrowFunctionExpression)
        {
            Id = null;
            Params = parameters;
            Body = body;
            Generator = false;
            Expression = expression;
            HoistingScope = hoistingScope;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Params, Body);
    }
}
