namespace Esprima.Ast;

/// <summary>
/// Represents either an <see cref="ExportSpecifier"/> or an <see cref="ImportDeclarationSpecifier"/>
/// </summary>
public interface IModuleSpecifier
{
    Nodes Type { get; }
    Expression Local { get; }
    ChildNodes ChildNodes { get; }
}
