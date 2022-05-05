using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassDeclaration : Declaration, IClass
    {
        internal readonly NodeList<Decorator> _decorators;

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
        public ReadOnlySpan<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _decorators.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Id;
            yield return SuperClass;
            yield return Body;

            foreach (var node in _decorators)
            {
                yield return node;
            }
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassDeclaration(this);
        }

        public ClassDeclaration UpdateWith(Identifier? id, Expression? superClass, ClassBody body, in NodeList<Decorator> decorators)
        {
            if (id == Id && superClass == SuperClass && body == Body && NodeList.AreSame(decorators, _decorators))
            {
                return this;
            }

            return new ClassDeclaration(id, superClass, body, decorators);
        }
    }
}
