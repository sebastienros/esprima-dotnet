namespace Esprima.Ast
{
    public sealed class NewExpression : Expression
    {
        private readonly NodeList<Expression> _arguments;
        public readonly Expression Callee;

        public NewExpression(
            Expression callee,
            in NodeList<Expression> args)
            : base(Nodes.NewExpression)
        {
            Callee = callee;
            _arguments = args;
        }

        public ref readonly NodeList<Expression> Arguments => ref _arguments;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Callee, Arguments);
    }
}
