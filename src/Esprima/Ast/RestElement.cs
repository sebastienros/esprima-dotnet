using System.Collections.Generic;

namespace Esprima.Ast
{
    public class RestElement : Node,
        IArrayPatternElement, Expression, ObjectPatternProperty
    {
        // Identifier in esprima but not forced and
        // for instance ...i[0] is a SpreadElement
        // which is reinterpreted to RestElement with a ComputerMemberExpression

        public readonly INode Argument; // BindingIdentifier | BindingPattern

        public RestElement(INode argument) :
            base(Nodes.RestElement)
        {
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}