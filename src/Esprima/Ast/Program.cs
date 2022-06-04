using Esprima.Utils;

namespace Esprima.Ast
{
    public abstract class Program : Statement
    {
        protected Program(Nodes type) : base(type)
        {
        }

        public abstract SourceType SourceType { get; }

        public abstract ref readonly NodeList<Statement> Body { get; }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitProgram(this);
        }

        protected abstract Program Rewrite(in NodeList<Statement> body);

        public Program UpdateWith(in NodeList<Statement> body)
        {
            if (NodeList.AreSame(body, Body))
            {
                return this;
            }

            return Rewrite(body);
        }
    }
}
