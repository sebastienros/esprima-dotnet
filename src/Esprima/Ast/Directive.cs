using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Directive : ExpressionStatement
    {
        [JsonProperty("directive")]
        public readonly string Directiv;

        public Directive(Expression expression, string directive)
            :base(expression)
        {
            Directiv = directive;
        }
    }
}
