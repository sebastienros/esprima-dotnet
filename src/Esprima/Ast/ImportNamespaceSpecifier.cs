namespace Esprima.Ast
{
    public class ImportNamespaceSpecifier : Node, ImportDeclarationSpecifier
    {
        public Identifier Local { get; }

        public ImportNamespaceSpecifier(Identifier local)
        {
            Type = Nodes.ImportNamespaceSpecifier;
            Local = local;
        }
    }
}