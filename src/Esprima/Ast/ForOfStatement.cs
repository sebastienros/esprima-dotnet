using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForOfStatement : Statement
    {
        public ForOfStatement(
            Node left,
            Expression right,
            Statement body,
            bool await) : base(Nodes.ForOfStatement)
        {
            Left = left;
            Right = right;
            Body = body;
            Await = await;
        }

        public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Await { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

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
