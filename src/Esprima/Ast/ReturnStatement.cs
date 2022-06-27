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

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator)
        {
            return enumerator.MoveNextOptional(Argument);
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitReturnStatement(this);
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
