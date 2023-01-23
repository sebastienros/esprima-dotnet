using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class RestElement : Node
{
    public RestElement(Node argument) : base(Nodes.RestElement)
    {
        Argument = argument;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="BindingPattern"/>
    /// </remarks>
    public Node Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RestElement Rewrite(Node argument)
    {
        return new RestElement(argument);
    }
}
