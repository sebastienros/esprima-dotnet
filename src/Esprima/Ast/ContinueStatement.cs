using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ContinueStatement : Statement
    {
        public readonly Identifier? Label;

        public ContinueStatement(Identifier? label) : base(Nodes.ContinueStatement)
        {
            Label = label;
        }

        public override NodeCollection ChildNodes => new(Label);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitContinueStatement(this) as T;
        }
    }
}
