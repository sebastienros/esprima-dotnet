namespace Esprima.Ast
{
    public class ForInStatement : Node,
        Statement
    {
        public INode Left;
        public Expression Right;
        public Statement Body;
        public bool Each;

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