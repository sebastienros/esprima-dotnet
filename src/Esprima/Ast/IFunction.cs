namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/>, a <see cref="FunctionExpression"/> or an <see cref="ArrowFunctionExpression"/>
    /// </summary>
    public interface IFunction
    {
        Identifier? Id { get; }
        ref readonly NodeList<Expression> Params { get; }
        Node Body { get; }
        bool Generator { get; }
        bool Expression { get; }
        bool Strict { get; }
        bool Async { get; }
        ChildNodes ChildNodes { get; }
    }
}
