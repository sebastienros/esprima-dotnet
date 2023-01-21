using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Properties) })]
public sealed class ObjectExpression : Expression
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Properties);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitObjectExpression(this);

    public ObjectExpression UpdateWith(in NodeList<Node> properties)
    {
        if (NodeList.AreSame(properties, Properties))
        {
            return this;
        }

        return new ObjectExpression(properties);
    }
}
