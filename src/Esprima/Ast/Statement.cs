namespace Esprima.Ast
{
    public class Statement : Node, INode, StatementListItem
    {
        public Identifier LabelSet { get; internal set; }
    }
}