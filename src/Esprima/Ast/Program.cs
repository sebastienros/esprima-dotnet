namespace Esprima.Ast
{
    public interface Program : INode
    {
        SourceType SourceType { get; }
        ref readonly NodeList<IStatementListItem> Body { get; }
    }
}