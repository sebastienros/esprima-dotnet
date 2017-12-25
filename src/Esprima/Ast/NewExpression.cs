using System.Collections.Generic;

namespace Esprima.Ast
{
    public class NewExpression : Node,
        Expression
    {
        public Expression Callee { get; }
        public List<ArgumentListElement> Arguments { get; }

        public NewExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.NewExpression;
            Callee = callee;
            Arguments = args;
        }

    }
}