using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportSpecifier : ImportDeclarationSpecifier
    {
        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression Imported;

        public ImportSpecifier(Identifier local, Expression imported) : base(local, Nodes.ImportSpecifier)
        {
            Imported = imported;
        }

        public override NodeCollection ChildNodes => new(Local, Imported);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitImportSpecifier(this) as T;
        }
    }
}
