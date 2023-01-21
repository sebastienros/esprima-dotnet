using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body) })]
public sealed class StaticBlock : ClassElement
{
    private readonly NodeList<Statement> _body;

    public StaticBlock(in NodeList<Statement> body) : base(Nodes.StaticBlock)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitStaticBlock(this);

    public StaticBlock UpdateWith(in NodeList<Statement> body)
    {
        if (NodeList.AreSame(body, Body))
        {
            return this;
        }

        return new StaticBlock(body);
    }
}
