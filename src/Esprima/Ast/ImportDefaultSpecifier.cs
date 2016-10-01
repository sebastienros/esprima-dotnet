namespace Esprima.Ast
{
    public class ImportDefaultSpecifier : Node,
        ImportDeclarationSpecifier
    {
        public Identifier Local;
        public ImportDefaultSpecifier(Identifier local)
        {
            Type = Nodes.ImportDefaultSpecifier;
            Local = local;
        }
    }
}
