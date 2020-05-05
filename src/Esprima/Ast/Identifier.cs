using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class Identifier : ArrayPatternElement
    {
        public readonly string Name;

        public Identifier(string name) : base(Nodes.Identifier)
        {
            Name = name;
        }

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}