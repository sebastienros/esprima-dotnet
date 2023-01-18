using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class MetaProperty : Expression
{
    public MetaProperty(Identifier meta, Identifier property) : base(Nodes.MetaProperty)
    {
        Meta = meta;
        Property = property;
    }

    public Identifier Meta { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Identifier Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Meta, Property);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitMetaProperty(this);

    public MetaProperty UpdateWith(Identifier meta, Identifier property)
    {
        if (meta == Meta && property == Property)
        {
            return this;
        }

        return new MetaProperty(meta, property);
    }
}
