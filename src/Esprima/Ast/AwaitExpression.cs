using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class AwaitExpression : Expression
    {
        public readonly Expression Argument;

        public AwaitExpression(Expression argument) : base(Nodes.AwaitExpression)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitAwaitExpression(this);
        }
    }
}
