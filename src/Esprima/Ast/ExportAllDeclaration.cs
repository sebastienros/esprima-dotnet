using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportAllDeclaration : ExportDeclaration
    {
        public readonly Literal Source;

        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression? Exported;

        public ExportAllDeclaration(Literal source) : this(source, null)
        {
        }

        public ExportAllDeclaration(Literal source, Expression? exported) : base(Nodes.ExportAllDeclaration)
        {
            Source = source;
            Exported = exported;
        }

        public override NodeCollection ChildNodes => new(Source, Exported);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitExportAllDeclaration(this) as T;
        }
    }
}
