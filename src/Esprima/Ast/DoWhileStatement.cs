namespace Esprima.Ast
{
    public class DoWhileStatement : Statement
    {
        public Statement Body { get; }
        public Expression Test { get; }

        public DoWhileStatement(Statement body, Expression test)
        {
            Type = Nodes.DoWhileStatement;
            Body = body;
            Test = test;
        }
    }
}