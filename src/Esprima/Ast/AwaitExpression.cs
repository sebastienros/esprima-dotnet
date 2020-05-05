using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class AwaitExpression : Expression
    {
        public readonly Expression Argument;

        public AwaitExpression(Expression argument) : base(Nodes.AwaitExpression)
        {
            Argument = argument;
        }

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}