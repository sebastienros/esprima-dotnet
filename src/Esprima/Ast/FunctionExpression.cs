namespace Esprima.Ast
{
    public sealed class FunctionExpression : Expression, IFunction
    {
        private readonly NodeList<Expression> _parameters;

        public FunctionExpression(
            Identifier? id,
            in NodeList<Expression> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async) :
            base(Nodes.FunctionExpression)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            Strict = strict;
            Async = async;
        }

        public Identifier? Id { get; }
        public ref readonly NodeList<Expression> Params => ref _parameters;
        public Node Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Async { get; }
        public bool Strict { get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Id, _parameters, Body);
    }
}