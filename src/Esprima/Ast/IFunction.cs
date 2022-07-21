namespace Esprima.Ast;

/// <summary>
/// Represents either a <see cref="FunctionDeclaration"/>, a <see cref="FunctionExpression"/> or an <see cref="ArrowFunctionExpression"/>
/// </summary>
public interface IFunction
{
    Nodes Type { get; }
    Identifier? Id { get; }
    ref readonly NodeList<Node> Params { get; }
    StatementListItem Body { get; }
    bool Generator { get; }
    bool Expression { get; }
    bool Strict { get; }
    bool Async { get; }
    ChildNodes ChildNodes { get; }
}
