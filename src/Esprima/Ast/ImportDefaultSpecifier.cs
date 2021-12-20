using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitImportDefaultSpecifier(this);
        }
    }
}
