using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportSpecifier : Statement
    {
        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression Exported;

        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression Local;

        public ExportSpecifier(Expression local, Expression exported) : base(Nodes.ExportSpecifier)
        {
            Exported = exported;
            Local = local;
        }

        public override NodeCollection ChildNodes => new(Exported, Local);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportSpecifier(this);
        }
    }
}
