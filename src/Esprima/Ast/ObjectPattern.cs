using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Properties) })]
public sealed partial class ObjectPattern : BindingPattern
{
    private readonly NodeList<Node> _properties;

    public ObjectPattern(in NodeList<Node> properties) : base(Nodes.ObjectPattern)
    {
        _properties = properties;
    }

    /// <summary>
    /// { <see cref="Property"/> | <see cref="RestElement"/> }
    /// </summary>
    public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ObjectPattern Rewrite(in NodeList<Node> properties)
    {
        return new ObjectPattern(properties);
    }
}
