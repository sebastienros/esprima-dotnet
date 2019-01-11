namespace Esprima.Ast
{
    public abstract class Statement : Node, INode, StatementListItem
    {
        public Identifier LabelSet { get; internal set; }
    }
}