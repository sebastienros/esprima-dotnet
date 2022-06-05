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

        public readonly NodeList<Decorator> Decorators;
        NodeList<Decorator> IClass.Decorators => Decorators;

        public ClassExpression(
            Identifier? id,
            Expression? superClass,
            ClassBody body,
            in NodeList<Decorator> decorators) : base(Nodes.ClassExpression)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
            Decorators = decorators;
        }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassExpression(this);
        }

        public ClassExpression UpdateWith(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators)
        {
            if (id == Id && superClass == SuperClass && body == Body && NodeList.AreSame(decorators, Decorators))
            {
                return this;
            }

            return new ClassExpression(id, superClass, body, Decorators);
        }

        private IEnumerable<Node> CreateChildNodes()
        {
            if (Id is not null)
            {
                yield return Id;
            }

            if (SuperClass is not null)
            {
                yield return SuperClass;
            }

            yield return Body;

            foreach (var node in Decorators)
            {
                yield return node;
            }
        }
    }
}
