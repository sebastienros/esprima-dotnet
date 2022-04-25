namespace Esprima.Ast
{
    public abstract class ImportDeclarationSpecifier : Declaration
    {
        public readonly Identifier Local;

        protected ImportDeclarationSpecifier(Identifier local, Nodes type) : base(type)
        {
            Local = local;
        }
    }
}
