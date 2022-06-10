using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class StaticBlock : Node
    {
        private readonly NodeList<Statement> _body;

        public StaticBlock(in NodeList<Statement> body) : base(Nodes.StaticBlock)
        {
            _body = body;
        }

        public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

        public sealed override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);

        protected internal sealed override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitStaticBlock(this);
        }

        public StaticBlock UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new StaticBlock(body);
        }
    }
}
