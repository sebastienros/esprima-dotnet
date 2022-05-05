using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public abstract class Program : Statement
    {
        internal readonly NodeList<Statement> _body;

        protected Program(in NodeList<Statement> body) : base(Nodes.Program)
        {
            _body = body;
        }

        public ReadOnlySpan<Statement> Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _body.AsSpan(); }

        public abstract SourceType SourceType { get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitProgram(this);
        }

        protected abstract Program Rewrite(in NodeList<Statement> body);

        public Program UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, _body))
            {
                return this;
            }

            return Rewrite(body);
        }
    }
}
