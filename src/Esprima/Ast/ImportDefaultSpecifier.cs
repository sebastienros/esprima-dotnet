namespace Esprima.Ast
{
    public class ImportDefaultSpecifier : Node, ImportDeclarationSpecifier
    {
        public readonly Identifier Local;

        public ImportDefaultSpecifier(Identifier local)
        {
            Type = Nodes.ImportDefaultSpecifier;
            Local = local;
        }
    }
}