using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassDeclaration : Declaration, IClass
    {
        public readonly Identifier? Id;
        Identifier? IClass.Id => Id;

        public readonly Expression? SuperClass; // Identifier || CallExpression
        Expression? IClass.SuperClass => SuperClass;

        public readonly ClassBody Body;
        ClassBody IClass.Body => Body;

        public ClassDeclaration(Identifier? id, Expression? superClass, ClassBody body) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Id, SuperClass, Body);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitClassDeclaration(this) as T;
        }
    }
}
