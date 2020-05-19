namespace Esprima.Ast
{
    public sealed class CatchClause : Statement
    {
        public readonly Expression Param; // BindingIdentifier | BindingPattern;
        public readonly BlockStatement Body;

        public CatchClause(Expression param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Param, Body);
    }
}