using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForInStatement : Statement
    {
        public ForInStatement(
            Node left,
            Expression right,
            Statement body) : base(Nodes.ForInStatement)
        {
            Left = left;
            Right = right;
            Body = body;
        }

        public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Left, Right, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitForInStatement(this);
        }

        public ForInStatement UpdateWith(Node left, Expression right, Statement body)
        {
            if (left == Left && right == Right && body == Body)
            {
                return this;
            }

            return new ForInStatement(left, right, body);
        }
    }
}
