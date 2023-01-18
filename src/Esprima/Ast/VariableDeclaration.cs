using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class VariableDeclaration : Declaration
{
    public static string GetVariableDeclarationKindToken(VariableDeclarationKind kind)
    {
        return kind switch
        {
            VariableDeclarationKind.Var => "var",
            VariableDeclarationKind.Let => "let",
            VariableDeclarationKind.Const => "const",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), "Invalid variable declaration kind: " + kind)
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Declarations);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);

    public VariableDeclaration UpdateWith(in NodeList<VariableDeclarator> declarations)
    {
        if (NodeList.AreSame(declarations, Declarations))
        {
            return this;
        }

        return new VariableDeclaration(declarations, Kind);
    }
}
