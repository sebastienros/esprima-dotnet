namespace Esprima.Ast
{
    public class ForStatement : Statement
    {
        // can be a Statement (var i) or an Expression (i=0)
        public INode Init { get; }
        public Expression Test { get; }
        public Expression Update { get; }
        public Statement Body { get; }

        public ForStatement(INode init, Expression test, Expression update, Statement body)
        {
            Type = Nodes.ForStatement;
            Init = init;
            Test = test;
            Update = update;
            Body = body;
        }
    }
}