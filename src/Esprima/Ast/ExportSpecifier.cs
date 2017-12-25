namespace Esprima.Ast
{
    public class ExportSpecifier : Statement
    {
        public Identifier Exported { get; }
        public Identifier Local { get; }

        public ExportSpecifier(Identifier local, Identifier exported)
        {
            Type = Nodes.ExportSpecifier;
            Exported = exported;
            Local = local;
        }
    }
}