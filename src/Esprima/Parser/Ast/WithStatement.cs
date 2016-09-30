namespace Esprima.Ast
{
    public class WithStatement : Node,
        Statement
    {
        public Expression Object;
        public Statement Body;

        public WithStatement(Expression obj, Statement body)
        {
            Type = Nodes.WithStatement;
            Object = obj;
            Body = body;
        }
    }
}