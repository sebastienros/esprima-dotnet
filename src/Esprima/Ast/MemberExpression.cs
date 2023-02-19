using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Object), nameof(Property) }, SealOverrideMethods = true)]
public abstract partial class MemberExpression : Expression
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

    protected abstract MemberExpression Rewrite(Expression @object, Expression property);
}
