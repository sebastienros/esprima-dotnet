namespace Esprima.Ast;

public sealed class ComputedMemberExpression : MemberExpression
{
    public ComputedMemberExpression(Expression obj, Expression property, bool optional)
        : base(obj, property, true, optional)
    {
    }

    protected override MemberExpression Rewrite(Expression obj, Expression property)
    {
        return new ComputedMemberExpression(obj, property, Optional);
    }
}
