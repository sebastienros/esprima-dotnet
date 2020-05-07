namespace Esprima.Ast
{
    public sealed class ImportSpecifier : ImportDeclarationSpecifier
    {
        public readonly Identifier Local;
        public readonly Identifier Imported;

        public ImportSpecifier(Identifier local, Identifier imported) : base(Nodes.ImportSpecifier)
        {
            Local = local;
            Imported = imported;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Local, Imported);
    }
}