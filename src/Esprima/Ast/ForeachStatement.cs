using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ForeachStatement : Statement
    {
        public readonly bool Await;
        public readonly Node Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForeachStatement(
            Node left,
            Expression right,
            Statement body,
            bool _await) : base(Nodes.ForeachStatement)
        {
            Left = left;
            Right = right;
            Body = body;
            Await = _await;
        }

        public override NodeCollection ChildNodes => new(Left, Right, Body);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitForOfStatement(this);
        }
    }
}
