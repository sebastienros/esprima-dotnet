using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportAllDeclaration : ExportDeclaration
    {
        public readonly Literal Source;

        public ExportAllDeclaration(Literal source) :
            base(Nodes.ExportAllDeclaration)
        {
            Source = source;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Source);
    }
}