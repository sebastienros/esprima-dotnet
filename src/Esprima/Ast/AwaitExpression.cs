using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class AwaitExpression : Node, Expression
    {
        private readonly Expression _argument;

        public AwaitExpression(Expression argument) : base(Nodes.AwaitExpression)
        {
            _argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_argument);
    }
}