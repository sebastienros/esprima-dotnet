namespace Esprima.Ast
{
    public class ForOfStatement : Node,
        Statement
    {
        public INode Left;
        public Expression Right;
        public Statement Body;
        public ForOfStatement(INode left, Expression right, Statement body)
        {
            Type = Nodes.ForOfStatement;
            Left = left;
            Right = right;
            Body = body;
        }
    }
}
