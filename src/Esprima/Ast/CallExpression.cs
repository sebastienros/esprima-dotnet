using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class CallExpression : Expression
    {
        private readonly NodeList<Expression> _arguments;

        public readonly Expression Callee;
        public readonly bool Optional;

        public CallExpression(
            Expression callee,
            in NodeList<Expression> args,
            bool optional) : base(Nodes.CallExpression)
        {
            Callee = callee;
            _arguments = args;
            Optional = optional;
        }

        public ref readonly NodeList<Expression> Arguments => ref _arguments;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Callee, _arguments);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitCallExpression(this);
        }

        public CallExpression UpdateWith(Expression callee, in NodeList<Expression> arguments)
        {
            if (callee == Callee && NodeList.AreSame(arguments, Arguments))
            {
                return this;
            }

            return new CallExpression(callee, arguments, Optional);
        }
    }
}
