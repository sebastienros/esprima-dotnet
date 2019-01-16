namespace Esprima.Ast
{
    public class DoWhileStatement : Statement
    {
        public readonly Statement Body;
        public readonly Expression Test;

        public DoWhileStatement(Statement body, Expression test) :
            base(Nodes.DoWhileStatement)
        {
            Body = body;
            Test = test;
        }
    }
}