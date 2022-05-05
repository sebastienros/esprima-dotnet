using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class StaticBlock : ClassElement
    {
        internal readonly NodeList<Statement> _body;

        public StaticBlock(in NodeList<Statement> body) : base(Nodes.StaticBlock)
        {
            _body = body;
        }

        public ReadOnlySpan<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _body.AsSpan(); }

        public sealed override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal sealed override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitStaticBlock(this);
        }

        public StaticBlock UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, _body))
            {
                return this;
            }

            return new StaticBlock(body);
        }
    }
}
