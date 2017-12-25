namespace Esprima.Ast
{
    public class ForInStatement : Statement
    {
        public INode Left { get; }
        public Expression Right { get; }
        public Statement Body { get; }
        public bool Each { get; }

        public ForInStatement(INode left, Expression right, Statement body)
        {
            Type = Nodes.ForInStatement;
            Left = left;
            Right = right;
            Body = body;
            Each = false;
        }
    }
}