using System.Collections.Generic;

namespace Esprima.Ast
{
    public class NewExpression : Node,
        Expression
    {
        public Expression Callee;
        public List<ArgumentListElement> Arguments;

        public NewExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.NewExpression;
            Callee = callee;
            Arguments = args;
        }

    }
}