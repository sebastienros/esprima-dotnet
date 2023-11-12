using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed partial class BlockStatement : Statement
{
    private readonly NodeList<Statement> _body;

    public BlockStatement(in NodeList<Statement> body) : base(Nodes.BlockStatement)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BlockStatement Rewrite(in NodeList<Statement> body)
    {
        return new BlockStatement(body);
    }
}
