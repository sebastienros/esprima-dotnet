namespace Esprima.Ast
{
    public class ForOfStatement : Statement
    {
        public INode Left { get; }
        public Expression Right { get; }
        public Statement Body { get; }

        public ForOfStatement(INode left, Expression right, Statement body)
        {
            Type = Nodes.ForOfStatement;
            Left = left;
            Right = right;
            Body = body;
        }
    }
}