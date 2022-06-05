using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportSpecifier : Statement
    {
        /// <summary>
        /// <see cref="Identifier" /> | <see cref="Literal" />
        /// </summary>
        public readonly Expression Exported;

        /// <summary>
        /// <see cref="Identifier" /> | StringLiteral <see cref="Literal" />
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

        public ExportSpecifier UpdateWith(Expression local, Expression exported)
        {
            if (local == Local && exported == Exported)
            {
                return this;
            }

            return new ExportSpecifier(local, exported);
        }
    }
}
