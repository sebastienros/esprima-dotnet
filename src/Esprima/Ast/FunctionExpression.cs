namespace Esprima.Ast
{
    public sealed class FunctionExpression : Expression, IFunction
    {
        private readonly NodeList<Expression> _parameters;

        public FunctionExpression(
            Identifier id,
            in NodeList<Expression> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async,
            HoistingScope hoistingScope) :
            base(Nodes.FunctionExpression)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            Strict = strict;
            Async = async;
            HoistingScope = hoistingScope;
        }

        public Identifier Id { get; }
        public ref readonly NodeList<Expression> Params => ref _parameters;
        public Node Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Async { get; }
        public bool Strict { get; }
        public HoistingScope HoistingScope { get; }

        public override NodeCollection ChildNodes => ChildNodeYielder.Yield(Id, _parameters, Body);
    }
}