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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitContinueStatement(this);
        }

        public ContinueStatement UpdateWith(Identifier? label)
        {
            if (label == Label)
            {
                return this;
            }

            return new ContinueStatement(label);
        }
    }
}
