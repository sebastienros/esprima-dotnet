using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Meta), nameof(Property) })]
public sealed partial class MetaProperty : Expression
{
    public MetaProperty(Identifier meta, Identifier property) : base(Nodes.MetaProperty)
    {
        Meta = meta;
        Property = property;
    }

    public Identifier Meta { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Identifier Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MetaProperty Rewrite(Identifier meta, Identifier property)
    {
        return new MetaProperty(meta, property);
    }
}
