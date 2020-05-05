using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ImportNamespaceSpecifier : ImportDeclarationSpecifier
    {
        public readonly Identifier Local;

        public ImportNamespaceSpecifier(Identifier local) :
            base(Nodes.ImportNamespaceSpecifier)
        {
            Local = local;
        }

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Local);
    }
}