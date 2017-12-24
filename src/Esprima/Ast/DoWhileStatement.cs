namespace Esprima.Ast
{
    public class DoWhileStatement : Statement
    {
        public readonly Statement Body;
        public readonly Expression Test;

        public DoWhileStatement(Statement body, Expression test)
        {
            Type = Nodes.DoWhileStatement;
            Body = body;
            Test = test;
        }
    }
}