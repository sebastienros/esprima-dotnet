using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ReturnStatement : Statement
    {
        public ReturnStatement(Expression? argument) : base(Nodes.ReturnStatement)
        {
            Argument = argument;
        }

        public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitReturnStatement(this, context);
        }

        public ReturnStatement UpdateWith(Expression? argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new ReturnStatement(argument);
        }
    }
}
