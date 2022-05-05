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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitProgram(this) as T;
        }
    }
}
