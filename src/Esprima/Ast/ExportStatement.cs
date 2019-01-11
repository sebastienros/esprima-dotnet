using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportStatement : Statement
    {
        public readonly Expression Expression;

        public ExportStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Expression);
   }
}