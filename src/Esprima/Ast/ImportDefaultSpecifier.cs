using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public new readonly Identifier Local;
        protected override Identifier LocalId => Local;

        public ImportDefaultSpecifier(Identifier local) : base(Nodes.ImportDefaultSpecifier)
        {
            Local = local;
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitImportDefaultSpecifier(this);
        }
    }
}
