using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ReturnStatement : Statement
    {
        public readonly Expression Argument;

        public ReturnStatement(Expression argument) :
            base(Nodes.ReturnStatement)
        {
            Argument = argument;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Argument);
    }
}