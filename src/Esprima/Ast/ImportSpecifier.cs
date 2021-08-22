using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportSpecifier : ImportDeclarationSpecifier
    {
        public new readonly Identifier Local;
        protected override Identifier LocalId => Local;

        public readonly Identifier Imported;

        public ImportSpecifier(Identifier local, Identifier imported) : base(Nodes.ImportSpecifier)
        {
            Local = local;
            Imported = imported;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Local, Imported);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitImportSpecifier(this);
    }
}