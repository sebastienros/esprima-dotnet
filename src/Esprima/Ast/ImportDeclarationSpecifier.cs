namespace Esprima.Ast
{
    public abstract class ImportDeclarationSpecifier : Declaration, IImportDeclarationSpecifier
    {
        protected ImportDeclarationSpecifier(Nodes type) : base(type)
        {
        }

        Identifier IImportDeclarationSpecifier.Local => LocalId;
        protected abstract Identifier LocalId { get; }
    }
}
