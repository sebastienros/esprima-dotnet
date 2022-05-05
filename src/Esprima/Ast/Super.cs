using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Super : Expression
    {
        public Super() : base(Nodes.Super)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitSuper(this) as T;
        }
    }
}
