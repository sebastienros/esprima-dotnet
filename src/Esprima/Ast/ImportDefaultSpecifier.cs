using System.Collections.Generic;

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

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Local);
    }
}