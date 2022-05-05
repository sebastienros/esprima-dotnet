using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        internal readonly NodeList<ClassElement> _body;

        public ClassBody(in NodeList<ClassElement> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        /// <remarks>
        /// <see cref="MethodDefinition" /> | <see cref="PropertyDefinition" /> | <see cref="StaticBlock" />
        /// </remarks>
        public ReadOnlySpan<ClassElement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _body.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassBody(this);
        }

        public ClassBody UpdateWith(in NodeList<ClassElement> body)
        {
            if (NodeList.AreSame(body, _body))
            {
                return this;
            }

            return new ClassBody(body);
        }
    }
}
