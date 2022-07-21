namespace Esprima.Ast;

public interface IProperty
{
    Nodes Type { get; }
    PropertyKind Kind { get; }
    Expression Key { get; }
    bool Computed { get; }
    Node? Value { get; }
    ChildNodes ChildNodes { get; }
}
