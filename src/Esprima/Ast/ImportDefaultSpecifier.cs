using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public ImportDefaultSpecifier(Identifier local) : base(local, Nodes.ImportDefaultSpecifier)
        {
        }

        public override NodeCollection ChildNodes => new(Local);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitImportDefaultSpecifier(this);
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
