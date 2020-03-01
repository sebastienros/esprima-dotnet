using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SpreadProperty : Node,
        ObjectExpressionProperty,
        ArgumentListElement,
        ArrayExpressionElement,
        Expression
    {
        public readonly Expression Argument;

        public SpreadProperty(Expression argument) :
            base(Nodes.SpreadProperty)
        {
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}