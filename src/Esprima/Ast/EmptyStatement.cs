using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;
        
        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitEmptyStatement(this) as T;
        }
    }
}
