using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Source) })]
public sealed partial class ImportExpression : Expression
{
    public ImportExpression(Expression source) : base(Nodes.ImportExpression)
    {
        Source = source;
    }

    public Expression Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ImportExpression Rewrite(Expression source)
    {
        return new ImportExpression(source);
    }
}
