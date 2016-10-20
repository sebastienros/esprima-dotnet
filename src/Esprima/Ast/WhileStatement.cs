namespace Esprima.Ast
{
    public class WhileStatement : Statement
    {
        public Expression Test;
        public Statement Body;

        public WhileStatement(Expression test, Statement body)
        {
            Type = Nodes.WhileStatement;
            Test = test;
            Body = body;
        }


    }
}