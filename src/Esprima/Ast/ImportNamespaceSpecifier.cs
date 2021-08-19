using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public readonly Identifier Local;
        protected override Identifier LocalId => Local;

        public ImportNamespaceSpecifier(Identifier local) : base(Nodes.ImportNamespaceSpecifier)
        {
            Local = local;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Local);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitImportNamespaceSpecifier(this);
    }
}