namespace Esprima.Ast
{
    public class NewExpression : Node,
        Expression
    {
        public readonly Expression Callee;
        public readonly List<ArgumentListElement> Arguments;

        public NewExpression(Expression callee, List<ArgumentListElement> args) :
            base(Nodes.NewExpression)
        {
            Callee = callee;
            Arguments = args;
        }

    }
}