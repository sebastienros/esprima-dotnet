using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public sealed class Directive : ExpressionStatement
    {
        public Directive(Expression expression, string value) : base(expression)
        {
            Value = value;
        }

        public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        protected override ExpressionStatement Rewrite(Expression expression)
        {
            return new Directive(expression, Value);
        }
    }
}
