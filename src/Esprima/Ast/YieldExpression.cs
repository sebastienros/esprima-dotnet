using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class YieldExpression : Expression
    {
        public YieldExpression(Expression? argument, bool delgate) : base(Nodes.YieldExpression)
        {
            Argument = argument;
            Delegate = delgate;
        }

        public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Delegate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitYieldExpression(this);
        }

        public YieldExpression UpdateWith(Expression? argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new YieldExpression(argument, Delegate);
        }
    }
}
