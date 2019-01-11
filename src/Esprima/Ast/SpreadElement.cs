using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SpreadElement : Node,
        ArgumentListElement,
        ArrayExpressionElement,
        Expression
    {
        public readonly Expression Argument;

        public SpreadElement(Expression argument)
        {
            Type = Nodes.SpreadElement;
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}