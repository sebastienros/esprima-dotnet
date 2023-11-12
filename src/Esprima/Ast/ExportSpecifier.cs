using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Local), nameof(Exported) })]
public sealed partial class ExportSpecifier : Node, IModuleSpecifier
{
    public ExportSpecifier(Expression local, Expression exported) : base(Nodes.ExportSpecifier)
    {
        Local = local;
        Exported = exported;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
    /// </remarks>
    public Expression Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextExportSpecifier(Local, Exported);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExportSpecifier Rewrite(Expression local, Expression exported)
    {
        return new ExportSpecifier(local, exported);
    }
}
