using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public readonly Expression? Source;

        public Import() : base(Nodes.Import)
        {
        }

        public Import(Expression? source) : base(Nodes.Import)
        {
            Source = source;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitImport(this) as T;
        }
    }
}
