using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public sealed class Directive : ExpressionStatement
    {
        public Directive(Expression expression, string directive) : base(expression)
        {
            Directiv = directive;
        }

        public string Directiv { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        protected override ExpressionStatement Rewrite(Expression expression)
        {
            return new Directive(expression, Directiv);
        }
    }
}
