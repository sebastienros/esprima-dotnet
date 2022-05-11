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

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitProgram(this);
        }
    }
}
