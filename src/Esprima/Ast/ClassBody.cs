using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ClassBody : Node
    {
        private readonly NodeList<Node> _body;

        public ClassBody(in NodeList<Node> body) : base(Nodes.ClassBody)
        {
            _body = body;
        }

        /// <remarks>
        /// <see cref="MethodDefinition" /> | <see cref="PropertyDefinition" /> | <see cref="StaticBlock" /> | <see cref="AccessorProperty" />
        /// </remarks>
        public ref readonly NodeList<Node> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitClassBody(this);
        }

        public ClassBody UpdateWith(in NodeList<Node> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new ClassBody(body).SetAdditionalInfo(this);
        }
    }
}
