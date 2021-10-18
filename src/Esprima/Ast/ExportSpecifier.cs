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

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitExportSpecifier(this);
        }
    }
}
