using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class ImportDefaultSpecifier : ImportDeclarationSpecifier
    {
        public readonly Identifier Local;

        public ImportDefaultSpecifier(Identifier local) : base(Nodes.ImportDefaultSpecifier)
        {
            Local = local;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Local);
    }
}