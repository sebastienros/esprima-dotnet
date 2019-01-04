namespace Esprima.Ast
{
    public class Directive : ExpressionStatement
    {
        public readonly string Directiv;

        public Directive(Expression expression, string directive)
            :base(expression)
        {
            Directiv = directive;
        }
    }
}
