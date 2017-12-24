namespace Esprima.Ast
{
    public class ImportNamespaceSpecifier : Node, ImportDeclarationSpecifier
    {
        public readonly Identifier Local;

        public ImportNamespaceSpecifier(Identifier local)
        {
            Type = Nodes.ImportNamespaceSpecifier;
            Local = local;
        }
    }
}