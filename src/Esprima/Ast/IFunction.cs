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
        bool Strict { get; }
    }
}
