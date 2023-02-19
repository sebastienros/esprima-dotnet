using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Id), nameof(Params), nameof(Body) })]
public sealed partial class FunctionExpression : Expression, IFunction
{
    private readonly NodeList<Node> _params;

    public FunctionExpression(
        Identifier? id,
        in NodeList<Node> parameters,
        BlockStatement body,
        bool generator,
        bool strict,
        bool async) :
        base(Nodes.FunctionExpression)
    {
        Id = id;
        _params = parameters;
        Body = body;
        Generator = generator;
        Strict = strict;
        Async = async;
    }

    public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <summary>
    /// { <see cref="Identifier"/> | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
    /// </summary>
    public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

    public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    StatementListItem IFunction.Body => Body;

    public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    bool IFunction.Expression => false;
    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FunctionExpression Rewrite(Identifier? id, in NodeList<Node> @params, BlockStatement body)
    {
        return new FunctionExpression(id, @params, body, Generator, Strict, Async);
    }
}
