using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Directive : ExpressionStatement
    {
        [JsonProperty("directive")]
        public string Directiv { get; }

        public Directive(Expression expression, string directive)
            :base(expression)
        {
            Directiv = directive;
        }
    }
}
