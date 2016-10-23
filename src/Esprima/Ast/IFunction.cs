using System.Collections.Generic;

namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/> or a <see cref="FunctionExpression"/>
    /// </summary>
    public interface IFunction
    {
        Identifier Id { get; set; }
        IEnumerable<INode> Params { get; set; }
        BlockStatement Body { get; set; }
        bool Generator { get; set; }
        bool Expression { get; set; }
        HoistingScope HoistingScope { get; }
    }

    public static class FunctionExtensions
    {
        public static bool IsStrict(this IFunction function)
        {
            foreach (var statement in function.Body.Body)
            {
                // A directive uses Tokens.Expression, so it can't
                // be detected using Type
                var directive = statement as Directive;
                if (directive == null)
                {
                    return false;
                }

                return directive.Directiv == "use strict";
            }

            return false;
        }
    }
}
