namespace Esprima.Ast
{
    public sealed class ExportAllDeclaration : ExportDeclaration
    {
        public readonly Literal Source;

        public ExportAllDeclaration(Literal source) : base(Nodes.ExportAllDeclaration)
        {
            Source = source;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Source);
    }
}