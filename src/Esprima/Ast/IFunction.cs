namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/> or a <see cref="FunctionExpression"/>
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
        NodeCollection ChildNodes { get; }
    }
}
