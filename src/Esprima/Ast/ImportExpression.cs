using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source), nameof(Attributes) })]
public sealed partial class ImportExpression : Expression
{
    public ImportExpression(Expression source) : this(source, null)
    {
    }

    public ImportExpression(Expression source, Expression? attributes) : base(Nodes.ImportExpression)
    {
        Source = source;
        Attributes = attributes;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Attributes { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportExpression Rewrite(Expression source, Expression? attributes)
    {
        return new ImportExpression(source, attributes);
    }
}
