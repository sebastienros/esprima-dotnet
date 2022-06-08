using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ImportAttribute : Node
{
    /// <summary>
    /// <see cref="Identifier" /> | <see cref="Literal" />
    /// </summary>
    public readonly Expression Key;
    public readonly Literal Value;

    public ImportAttribute(Expression key, Literal value) : base(Nodes.ImportAttribute)
    {
        Key = key;
        Value = value;
    }

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

        return new ImportAttribute(key, value).SetAdditionalInfo(this);
    }
}
