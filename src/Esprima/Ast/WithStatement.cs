namespace Esprima.Ast
{
    public class WithStatement : Statement
    {
        public Expression Object { get; }
        public Statement Body { get; }

        public WithStatement(Expression obj, Statement body)
        {
            Type = Nodes.WithStatement;
            Object = obj;
            Body = body;
        }
    }
}