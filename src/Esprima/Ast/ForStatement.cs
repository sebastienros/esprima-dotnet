namespace Esprima.Ast
{
    public sealed class ForStatement : Statement
    {
        // can be a Statement (var i) or an Expression (i=0)
        public readonly StatementListItem? Init;
        public readonly Expression? Test;
        public readonly Expression? Update;
        public readonly Statement Body;

        public ForStatement(
            StatementListItem? init,
            Expression? test,
            Expression? update, 
            Statement body) 
            : base(Nodes.ForStatement)
        {
            Init = init;
            Test = test;
            Update = update;
            Body = body;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Init, Test, Update, Body);
    }
}