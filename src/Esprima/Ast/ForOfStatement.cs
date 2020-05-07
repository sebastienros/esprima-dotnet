namespace Esprima.Ast
{
    public sealed class ForOfStatement : Statement
    {
        public readonly Node Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForOfStatement(
            Node left, 
            Expression right,
            Statement body) : base(Nodes.ForOfStatement)
        {
            Left = left;
            Right = right;
            Body = body;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Left, Right, Body); 
    }
}