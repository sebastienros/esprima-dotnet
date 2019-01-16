using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression) :
            base(Nodes.ExpressionStatement)
        {
            Expression = expression;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Expression);
    }
}