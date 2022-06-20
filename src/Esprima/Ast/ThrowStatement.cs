using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ThrowStatement : Statement
    {
        public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
        {
            Argument = argument;
        }

        public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitThrowStatement(this, context);
        }

        public ThrowStatement UpdateWith(Expression argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new ThrowStatement(argument);
        }

    }
}
