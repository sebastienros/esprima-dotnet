using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportAllDeclaration : ExportDeclaration
    {
        public readonly Literal Source;
        public readonly Identifier? Exported;

        public ExportAllDeclaration(Literal source) : this(source, null)
        {
        }

        public ExportAllDeclaration(Literal source, Identifier? exported) : base(Nodes.ExportAllDeclaration)
        {
            Source = source;
            Exported = exported;
        }

        public override NodeCollection ChildNodes => new(Source, Exported);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitExportAllDeclaration(this);
        }
    }
}
