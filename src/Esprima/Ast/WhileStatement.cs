namespace Esprima.Ast
{
    public class WhileStatement : Statement
    {
        public Expression Test { get; }
        public Statement Body { get; }

        public WhileStatement(Expression test, Statement body)
        {
            Type = Nodes.WhileStatement;
            Test = test;
            Body = body;
        }
    }
}