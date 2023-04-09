using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source), nameof(Options) })]
public sealed partial class ImportExpression : Expression
{
    public ImportExpression(Expression source) : this(source, null)
    {
    }

    public ImportExpression(Expression source, Expression? options) : base(Nodes.ImportExpression)
    {
        Source = source;
        Options = options;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Options { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportExpression Rewrite(Expression source, Expression? options)
    {
        return new ImportExpression(source, options);
    }
}
