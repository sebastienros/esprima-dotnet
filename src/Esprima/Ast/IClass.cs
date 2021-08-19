namespace Esprima.Ast
{
    /// <summary>
    /// Represents either a <see cref="ClassDeclaration"/> or an <see cref="ClassExpression"/>
    /// </summary>
    public interface IClass
    {
        Identifier? Id { get; }
        Expression? SuperClass { get; }
        ClassBody Body { get; }
        NodeCollection ChildNodes { get; }
    }
}
