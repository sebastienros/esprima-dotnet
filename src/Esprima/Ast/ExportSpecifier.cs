using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportSpecifier : Statement
    {
        public readonly Identifier Exported;
        public readonly Identifier Local;

        public ExportSpecifier(Identifier local, Identifier exported) : base(Nodes.ExportSpecifier)
        {
            Exported = exported;
            Local = local;
        }

        public override NodeCollection ChildNodes => new(Exported, Local);

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportSpecifier(this);
        }
    }
}
