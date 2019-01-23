namespace Esprima.Ast
{
    public abstract class Statement : Node, INode, IStatementListItem
    {
        protected Statement(Nodes type) : base(type) {}

        public Identifier LabelSet { get; internal set; }
    }
}