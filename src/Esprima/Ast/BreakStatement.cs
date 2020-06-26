namespace Esprima.Ast
{
    public sealed class BreakStatement : Statement
    {
        public readonly Identifier? Label;

        public BreakStatement(Identifier? label) : base(Nodes.BreakStatement)
        {
            Label = label;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Label);
    }
}