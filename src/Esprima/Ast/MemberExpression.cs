using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public abstract class MemberExpression : Expression
{
    protected MemberExpression(Expression obj, Expression property, bool computed, bool optional)
        : base(Nodes.MemberExpression)
    {
        Object = obj;
        Property = property;
        Computed = computed;
        Optional = optional;
    }

    public Expression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Property { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <summary>
    /// True if an indexer is used and the property to be evaluated.
    /// </summary>
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Optional { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal sealed override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Object, Property);

    protected internal sealed override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitMemberExpression(this);

    protected abstract MemberExpression Rewrite(Expression obj, Expression property);

    public MemberExpression UpdateWith(Expression obj, Expression property)
    {
        if (obj == Object && property == Property)
        {
            return this;
        }

        return Rewrite(obj, property);
    }
}
