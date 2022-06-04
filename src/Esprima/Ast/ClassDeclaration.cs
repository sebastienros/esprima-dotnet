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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassDeclaration(this);
        }

        public ClassDeclaration UpdateWith(Identifier? id, Expression? superClass, ClassBody body)
        {
            if (id == Id && superClass == SuperClass && body == Body)
            {
                return this;
            }

            return new ClassDeclaration(id, superClass, body);
        }
    }
}
