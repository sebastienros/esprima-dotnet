using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class ThrowStatement : Statement
    {
        public readonly Expression Argument;

        public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
        {
            Argument = argument;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Argument);
    }
}