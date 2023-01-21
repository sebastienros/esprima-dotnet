using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source), nameof(Attributes) })]
public sealed class Import : Expression
{
    public Import(Expression source) : this(source, null)
    {
    }

    public Import(Expression source, Expression? attributes) : base(Nodes.Import)
    {
        Source = source;
        Attributes = attributes;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Source, Attributes);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImport(this);

    public Import UpdateWith(Expression source, Expression? attributes)
    {
        if (source == Source && attributes == Attributes)
        {
            return this;
        }

        return new Import(source, attributes);
    }
}
