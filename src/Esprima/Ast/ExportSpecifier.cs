using System;

namespace Esprima.Ast
{
    public class ExportSpecifier : Node, Statement
    {
        public Identifier Exported;
        public Identifier Local;
        public ExportSpecifier(Identifier local, Identifier exported)
        {
            Type = Nodes.ExportSpecifier;
            Exported = exported;
            Local = local;
        }
    }
}
