using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed partial class StaticBlock : ClassElement
{
    private readonly NodeList<Statement> _body;

    public StaticBlock(in NodeList<Statement> body) : base(Nodes.StaticBlock)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StaticBlock Rewrite(in NodeList<Statement> body)
    {
        return new StaticBlock(body);
    }
}
