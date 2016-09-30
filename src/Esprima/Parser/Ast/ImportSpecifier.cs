namespace Esprima.Ast
{
    public class ImportSpecifier : Node,
        ImportDeclarationSpecifier
    {
        public Identifier Local;
        public Identifier Imported;
        public ImportSpecifier(Identifier local, Identifier imported)
        {
            Type = Nodes.ImportSpecifier;
            Local = local;
            Imported = imported;
        }
    }
}
