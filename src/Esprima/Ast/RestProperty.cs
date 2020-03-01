using System.Collections.Generic;

namespace Esprima.Ast
{
    public class RestProperty : Node,
        IArrayPatternElement, Expression, ObjectPatternProperty
    {
        public readonly INode Argument;

        public RestProperty(INode argument) :
            base(Nodes.RestProperty)
        {
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}