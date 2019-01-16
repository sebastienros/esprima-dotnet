using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ImportNamespaceSpecifier : Node, ImportDeclarationSpecifier
    {
        public readonly Identifier Local;

        public ImportNamespaceSpecifier(Identifier local) :
            base(Nodes.ImportNamespaceSpecifier)
        {
            Local = local;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Local);
    }
}