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

    public override NodeCollection ChildNodes => new(Key, Value);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitImportAttribute(this);
    }

    public ImportAttribute UpdateWith(Expression key, Literal value)
    {
        if (key == Key && value == Value)
        {
            return this;
        }

        return new ImportAttribute(key, value);
    }
}
