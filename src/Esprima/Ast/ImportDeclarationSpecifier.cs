namespace Esprima.Ast
{
    public abstract class ImportDeclarationSpecifier : Declaration
    {
        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression Local;

        protected ImportDeclarationSpecifier(Expression local, Nodes type) : base(type)
        {
            Local = local;
        }
    }
}
