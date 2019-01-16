namespace Esprima.Ast
{
    public class WithStatement : Statement
    {
        public readonly Expression Object;
        public readonly Statement Body;

        public WithStatement(Expression obj, Statement body) :
            base(Nodes.WithStatement)
        {
            Object = obj;
            Body = body;
        }
    }
}