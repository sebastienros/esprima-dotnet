using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ThrowStatement : Statement
    {
        public readonly Expression Argument;

        public ThrowStatement(Expression argument) :
            base(Nodes.ThrowStatement)
        {
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}