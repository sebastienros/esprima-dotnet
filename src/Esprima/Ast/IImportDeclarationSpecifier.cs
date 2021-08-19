namespace Esprima.Ast
{
    public interface IImportDeclarationSpecifier
    {
        Identifier Local { get; }
        NodeCollection ChildNodes { get; }
    }
}