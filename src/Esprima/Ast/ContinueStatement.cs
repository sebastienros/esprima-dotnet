namespace Esprima.Ast
{
    public sealed class ContinueStatement : Statement
    {
        public readonly Identifier? Label;

        public ContinueStatement(Identifier? label) : base(Nodes.ContinueStatement)
        {
            Label = label;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Label);
    }
}