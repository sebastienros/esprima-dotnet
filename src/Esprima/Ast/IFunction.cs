using System.Collections.Generic;

namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/> or a <see cref="FunctionExpression"/>
    /// </summary>
    public interface IFunction
    {
        Identifier Id { get; }
        List<INode> Params { get; }
        BlockStatement Body { get; }
        bool Generator { get; }
        bool Expression { get; }
        HoistingScope HoistingScope { get; }
        bool Strict { get; }
    }
}
