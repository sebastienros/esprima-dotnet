using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitImportNamespaceSpecifier(this);
        }
    }
}
