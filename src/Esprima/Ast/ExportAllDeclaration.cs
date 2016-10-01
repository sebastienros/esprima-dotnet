namespace Esprima.Ast
{
    public class ExportAllDeclaration : Node,
        ExportDeclaration
    {
        public Literal Source;
        public ExportAllDeclaration(Literal source)
        {
            Type = Nodes.ExportAllDeclaration;
            Source = source;
        }
    }
}
