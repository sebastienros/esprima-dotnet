using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForOfStatement : Statement
    {
        public readonly bool Await;
        public readonly Node Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForOfStatement(
            Node left,
            Expression right,
            Statement body,
            bool _await) : base(Nodes.ForOfStatement)
        {
            Left = left;
            Right = right;
            Body = body;
            Await = _await;
        }

        public override NodeCollection ChildNodes => new(Left, Right, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitForOfStatement(this);
        }

        public ForOfStatement UpdateWith(Node left, Expression right, Statement body)
        {
            if (left == Left && right == Right && body == Body)
            {
                return this;
            }

            return new ForOfStatement(left, right, body, Await);
        }
    }
}
