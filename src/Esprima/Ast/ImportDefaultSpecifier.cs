using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitImportDefaultSpecifier(this);
        }
    }
}
