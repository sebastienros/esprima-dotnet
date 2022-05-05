using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class BreakStatement : Statement
    {
        public readonly Identifier? Label;

        public BreakStatement(Identifier? label) : base(Nodes.BreakStatement)
        {
            Label = label;
        }

        public override NodeCollection ChildNodes => new(Label);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitBreakStatement(this) as T;
        }
    }
}
