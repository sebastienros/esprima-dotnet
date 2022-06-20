using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class BlockStatement : Statement
    {
        private readonly NodeList<Statement> _body;

        public BlockStatement(in NodeList<Statement> body) : base(Nodes.BlockStatement)
        {
            _body = body;
        }

        public ref readonly NodeList<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _body; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitBlockStatement(this, context);
        }

        public BlockStatement UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return new BlockStatement(body);
        }
    }
}
