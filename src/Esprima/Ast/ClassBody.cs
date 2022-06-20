using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<ClassElement> _body;

        public ClassBody(in NodeList<ClassElement> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        /// <remarks>
        /// <see cref="MethodDefinition" /> | <see cref="PropertyDefinition" /> | <see cref="StaticBlock" />
        /// </remarks>
        public ref readonly NodeList<ClassElement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitClassBody(this, context);
        }

        public ClassBody UpdateWith(in NodeList<ClassElement> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new ClassBody(body);
        }
    }
}
