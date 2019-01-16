namespace Esprima.Ast
{
    public class ExportAllDeclaration : Node, ExportDeclaration
    {
        public readonly Literal Source;

        public ExportAllDeclaration(Literal source) :
            base(Nodes.ExportAllDeclaration)
        {
            Source = source;
        }
    }
}