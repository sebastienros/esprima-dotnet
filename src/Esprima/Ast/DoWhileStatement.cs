namespace Esprima.Ast
{
    public class DoWhileStatement : Node,
        Statement
    {
        public Statement Body;
        public Expression Test;

        public DoWhileStatement(Statement body, Expression test)
        {
            Type = Nodes.DoWhileStatement;
            Body = body;
            Test = test;
        }

    }
}