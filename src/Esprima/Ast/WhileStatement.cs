namespace Esprima.Ast
{
    public class WhileStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Body;

        public WhileStatement(Expression test, Statement body) :
            base(Nodes.WhileStatement)
        {
            Test = test;
            Body = body;
        }
    }
}