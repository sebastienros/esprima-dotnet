namespace Esprima.Ast;

public sealed class ComputedMemberExpression : MemberExpression
{
    public ComputedMemberExpression(Expression obj, Expression property, bool optional)
        : base(obj, property, true, optional)
    {
    }

    protected override MemberExpression Rewrite(Expression @object, Expression property)
    {
        return new ComputedMemberExpression(@object, property, Optional);
    }
}
