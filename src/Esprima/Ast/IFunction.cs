namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/>, a <see cref="FunctionExpression"/> or an <see cref="ArrowFunctionExpression"/>
    /// </summary>
    public interface IFunction
    {
        Identifier? Id { get; }
        ReadOnlySpan<Expression> Params { get; }
        Node Body { get; }
        bool Generator { get; }
        bool Expression { get; }
        bool Strict { get; }
        bool Async { get; }
        NodeCollection ChildNodes { get; }
    }
}
