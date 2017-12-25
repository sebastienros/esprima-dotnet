namespace Esprima.Ast
{
    public class ImportSpecifier : Node, ImportDeclarationSpecifier
    {
        public Identifier Local { get; }
        public Identifier Imported { get; }

        public ImportSpecifier(Identifier local, Identifier imported)
        {
            Type = Nodes.ImportSpecifier;
            Local = local;
            Imported = imported;
        }
    }
}