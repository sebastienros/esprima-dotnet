using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public abstract class Program : Node
{
    private readonly NodeList<Statement> _body;

    protected Program(in NodeList<Statement> body) : base(Nodes.Program)
    {
        _body = body;
    }

    public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

    public abstract SourceType SourceType { get; }

    internal sealed override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body);

    protected internal sealed override object? Accept(AstVisitor visitor) => visitor.VisitProgram(this);

    protected abstract Program Rewrite(in NodeList<Statement> body);

    public Program UpdateWith(in NodeList<Statement> body)
    {
        if (NodeList.AreSame(body, Body))
        {
            return this;
        }

        return Rewrite(body);
    }
}
