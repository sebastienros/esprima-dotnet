namespace Esprima.Ast
{
    public interface IProperty
    {
        PropertyKind Kind { get; }
        Expression Key { get; }
        bool Computed { get; }
        Expression? Value { get; }
    }
}
