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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitAwaitExpression(this) as T;
        }
    }
}
