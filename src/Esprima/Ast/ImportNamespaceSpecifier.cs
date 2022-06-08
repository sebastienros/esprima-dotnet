using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public ImportNamespaceSpecifier(Identifier local) : base(local, Nodes.ImportNamespaceSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitImportNamespaceSpecifier(this);
        }

        public ImportNamespaceSpecifier UpdateWith(Identifier local)
        {
            if (local == Local)
            {
                return this;
            }

            return new ImportNamespaceSpecifier(local).SetAdditionalInfo(this);
        }
    }
}
