using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassDeclaration : Declaration, IClass
    {
        private readonly NodeList<Decorator> _decorators;

        public ClassDeclaration(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
            _decorators = decorators;
        }

        public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// <see cref="Identifier" /> | <see cref="CallExpression" />
        /// </remarks>
        public Expression? SuperClass { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ClassBody Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

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
