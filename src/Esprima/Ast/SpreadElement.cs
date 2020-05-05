using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SpreadElement : Expression
    {
        public readonly Expression Argument;

        public SpreadElement(Expression argument) :
            base(Nodes.SpreadElement)
        {
            Argument = argument;
        }

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}