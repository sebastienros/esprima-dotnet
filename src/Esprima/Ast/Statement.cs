namespace Esprima.Ast
{
    public class Statement : Node, INode, StatementListItem
    {
        protected Statement(Nodes type) : base(type) {}

        public Identifier LabelSet { get; internal set; }
    }
}