using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitEmptyStatement(this);
    }
}
