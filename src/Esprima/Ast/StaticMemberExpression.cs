namespace Esprima.Ast;

public sealed class StaticMemberExpression : MemberExpression
{
    public StaticMemberExpression(Expression obj, Expression property, bool optional)
        : base(obj, property, false, optional)
    {
    }

    protected override MemberExpression Rewrite(Expression @object, Expression property)
    {
        return new StaticMemberExpression(@object, property, Optional);
    }
}
