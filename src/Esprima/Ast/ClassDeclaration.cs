using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassDeclaration : Declaration, IClass
    {
        public readonly Identifier? Id;
        Identifier? IClass.Id => Id;

        /// <summary>
        /// <see cref="Identifier" /> | <see cref="CallExpression" />
        /// </summary>
        public readonly Expression? SuperClass;
        Expression? IClass.SuperClass => SuperClass;

        public readonly ClassBody Body;
        ClassBody IClass.Body => Body;

        public readonly NodeList<Decorator> Decorators;
        NodeList<Decorator> IClass.Decorators => Decorators;

        public ClassDeclaration(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
            Decorators = decorators;
        }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassDeclaration(this);
        }

        public ClassDeclaration UpdateWith(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators)
        {
            if (id == Id && superClass == SuperClass && body == Body && NodeList.AreSame(decorators, Decorators))
            {
                return this;
            }

            return new ClassDeclaration(id, superClass, body, decorators).SetAdditionalInfo(this);
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
