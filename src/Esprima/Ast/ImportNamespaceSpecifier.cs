using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitImportNamespaceSpecifier(this) as T;
        }
    }
}
