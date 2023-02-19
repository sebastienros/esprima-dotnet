using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Params), nameof(Body) })]
public sealed partial class ArrowFunctionExpression : Expression, IFunction
{
    private readonly NodeList<Node> _params;

    public ArrowFunctionExpression(
        in NodeList<Node> parameters,
        StatementListItem body,
        bool expression,
        bool strict,
        bool async)
        : base(Nodes.ArrowFunctionExpression)
    {
        _params = parameters;
        Body = body;
        Expression = expression;
        Strict = strict;
        Async = async;
    }

    Identifier? IFunction.Id => null;
    /// <summary>
    /// { <see cref="Identifier"/> | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
    /// </summary>
    public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }
    /// <remarks>
    /// <see cref="BlockStatement"/> | <see cref="Ast.Expression"/>
    /// </remarks>
    public StatementListItem Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    bool IFunction.Generator => false;
    public bool Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ArrowFunctionExpression Rewrite(in NodeList<Node> @params, StatementListItem body)
    {
        return new ArrowFunctionExpression(@params, body, Expression, Strict, Async);
    }
}
