namespace Esprima.Ast
{
    public class ExportSpecifier : Statement
    {
        public readonly Identifier Exported;
        public readonly Identifier Local;

        public ExportSpecifier(Identifier local, Identifier exported)
        {
            Type = Nodes.ExportSpecifier;
            Exported = exported;
            Local = local;
        }
    }
}