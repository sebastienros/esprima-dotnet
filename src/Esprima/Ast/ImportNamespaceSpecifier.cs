using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitImportNamespaceSpecifier(this);
        }
    }
}
