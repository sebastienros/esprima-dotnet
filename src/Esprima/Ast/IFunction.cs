namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="FunctionDeclaration"/> or a <see cref="FunctionExpression"/>
    /// </summary>
    public interface IFunction : INode
    {
        Identifier Id { get; }
        NodeList<INode> Params { get; }
        INode Body { get; }
        bool Generator { get; }
        bool Expression { get; }
        HoistingScope HoistingScope { get; }
        bool Strict { get; }
    }
}
