using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public new readonly Identifier Local;
        protected override Identifier LocalId => Local;

        public ImportNamespaceSpecifier(Identifier local) : base(Nodes.ImportNamespaceSpecifier)
        {
            Local = local;
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitImportNamespaceSpecifier(this);
        }
    }
}
