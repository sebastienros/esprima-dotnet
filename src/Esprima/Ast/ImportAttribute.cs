using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ImportAttribute : Node
{
    public ImportAttribute(Expression key, Literal value) : base(Nodes.ImportAttribute)
    {
        Key = key;
        Value = value;
    }

    /// <remarks>
    /// <see cref="Identifier" /> | <see cref="Literal" />
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Literal Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Key, Value);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImportAttribute(this);

    public ImportAttribute UpdateWith(Expression key, Literal value)
    {
        if (key == Key && value == Value)
        {
            return this;
        }

        return new ImportAttribute(key, value);
    }
}
