using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Declarations) })]
public sealed partial class VariableDeclaration : Declaration
{
    public static string GetVariableDeclarationKindToken(VariableDeclarationKind kind)
    {
        return kind switch
        {
            VariableDeclarationKind.Var => "var",
            VariableDeclarationKind.Let => "let",
            VariableDeclarationKind.Const => "const",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Invalid variable declaration kind.")
        };
    }

    private readonly NodeList<VariableDeclarator> _declarations;

    public VariableDeclaration(
        in NodeList<VariableDeclarator> declarations,
        VariableDeclarationKind kind)
        : base(Nodes.VariableDeclaration)
    {
        _declarations = declarations;
        Kind = kind;
    }

    public ref readonly NodeList<VariableDeclarator> Declarations { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _declarations; }
    public VariableDeclarationKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private VariableDeclaration Rewrite(in NodeList<VariableDeclarator> declarations)
    {
        return new VariableDeclaration(declarations, Kind);
    }
}
