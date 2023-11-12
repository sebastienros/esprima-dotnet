using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declaration) })]
public sealed partial class ExportDefaultDeclaration : ExportDeclaration
{
    public ExportDefaultDeclaration(StatementListItem declaration) : base(Nodes.ExportDefaultDeclaration)
    {
        Declaration = declaration;
    }

    /// <remarks>
    /// <see cref="Expression"/> | <see cref="ClassDeclaration"/> | <see cref="FunctionDeclaration"/>
    /// </remarks>
    public StatementListItem Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ExportDefaultDeclaration Rewrite(StatementListItem declaration)
    {
        return new ExportDefaultDeclaration(declaration);
    }
}
