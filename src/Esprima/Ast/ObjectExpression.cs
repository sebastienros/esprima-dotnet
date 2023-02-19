using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Properties) })]
public sealed partial class ObjectExpression : Expression
{
    private readonly NodeList<Node> _properties;

    public ObjectExpression(in NodeList<Node> properties) : base(Nodes.ObjectExpression)
    {
        _properties = properties;
    }

    /// <summary>
    /// { <see cref="Property"/> | <see cref="SpreadElement"/> }
    /// </summary>
    public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectExpression Rewrite(in NodeList<Node> properties)
    {
        return new ObjectExpression(properties);
    }
}
