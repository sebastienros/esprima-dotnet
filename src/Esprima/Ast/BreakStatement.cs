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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitBreakStatement(this);
        }

        public BreakStatement UpdateWith(Identifier? label)
        {
            if (label == Label)
            {
                return this;
            }

            return new BreakStatement(label).SetAdditionalInfo(this);
        }
    }
}
