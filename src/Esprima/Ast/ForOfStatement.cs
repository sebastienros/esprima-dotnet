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

        public override NodeCollection ChildNodes => new NodeCollection(Left, Right, Body); 
    }
}