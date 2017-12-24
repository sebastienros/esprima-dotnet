namespace Esprima.Ast
{
    public class ForInStatement : Statement
    {
        public readonly INode Left;
        public readonly Expression Right;
        public readonly Statement Body;
        public readonly bool Each;

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