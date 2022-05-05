using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class NewExpression : Expression
    {
        internal readonly NodeList<Expression> _arguments;

        public NewExpression(
            Expression callee,
            in NodeList<Expression> args)
            : base(Nodes.NewExpression)
        {
            Callee = callee;
            _arguments = args;
        }

        public Expression Callee { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<Expression> Arguments { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _arguments.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Callee, _arguments);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitNewExpression(this);
        }

        public NewExpression UpdateWith(Expression callee, in NodeList<Expression> arguments)
        {
            if (callee == Callee && NodeList.AreSame(arguments, _arguments))
            {
                return this;
            }

            return new NewExpression(callee, arguments);
        }
    }
}
