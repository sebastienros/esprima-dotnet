namespace Esprima.Ast
{
    public class ForOfStatement : Statement
    {
        public readonly INode Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForOfStatement(INode left, Expression right, Statement body)
        {
            Type = Nodes.ForOfStatement;
            Left = left;
            Right = right;
            Body = body;
        }
    }
}