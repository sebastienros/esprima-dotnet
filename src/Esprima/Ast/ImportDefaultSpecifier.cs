using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitImportDefaultSpecifier(this) as T;
        }
    }
}
