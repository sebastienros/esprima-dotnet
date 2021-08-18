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

        public override NodeCollection ChildNodes => new NodeCollection(Argument);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitAwaitExpression(this);
    }
}