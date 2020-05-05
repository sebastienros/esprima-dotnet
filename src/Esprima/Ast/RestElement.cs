using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class RestElement : ArrayPatternElement
    {
        // Identifier in esprima but not forced and
        // for instance ...i[0] is a SpreadElement
        // which is reinterpreted to RestElement with a ComputerMemberExpression

        public readonly Node Argument; // BindingIdentifier | BindingPattern

        public RestElement(Node argument) : base(Nodes.RestElement)
        {
            Argument = argument;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Argument);
    }
}