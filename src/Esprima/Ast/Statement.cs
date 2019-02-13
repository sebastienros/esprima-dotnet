namespace Esprima.Ast
{
    public abstract class Statement : Node, INode, StatementListItem
    {
        protected Statement(Nodes type) : base(type) {}

        public Identifier LabelSet { get; internal set; }
    }
}