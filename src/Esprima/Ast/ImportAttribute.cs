using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class ImportAttribute : Node
{
    public ImportAttribute(Expression key, Literal value) : base(Nodes.ImportAttribute)
    {
        Key = key;
        Value = value;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/>
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Literal Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportAttribute Rewrite(Expression key, Literal value)
    {
        return new ImportAttribute(key, value);
    }
}
