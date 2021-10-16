namespace Esprima.Ast
{
    public abstract class ImportDeclarationSpecifier : Declaration
    {
        protected ImportDeclarationSpecifier(Nodes type) : base(type)
        {
        }

        public Identifier Local => LocalId;
        protected abstract Identifier LocalId { get; }
    }
}
