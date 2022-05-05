using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassExpression : Expression, IClass
    {
        public readonly Identifier? Id;
        Identifier? IClass.Id => Id;

        public readonly Expression? SuperClass;
        Expression? IClass.SuperClass => SuperClass;

        public readonly ClassBody Body;
        ClassBody IClass.Body => Body;

        public ClassExpression(
            Identifier? id,
            Expression? superClass,
            ClassBody body) : base(Nodes.ClassExpression)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Id, SuperClass, Body);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitClassExpression(this) as T;
        }
    }
}
