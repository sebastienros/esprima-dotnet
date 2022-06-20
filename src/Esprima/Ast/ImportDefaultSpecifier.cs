using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitImportDefaultSpecifier(this, context);
        }

        public ImportDefaultSpecifier UpdateWith(Identifier local)
        {
            if (local == Local)
            {
                return this;
            }

            return new ImportDefaultSpecifier(local);
        }
    }
}
