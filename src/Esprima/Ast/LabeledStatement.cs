using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class LabeledStatement : Statement
    {
        public readonly Identifier Label;
        public readonly Statement Body;

        public LabeledStatement(Identifier label, Statement body) : base(Nodes.LabeledStatement)
        {
            Label = label;
            Body = body;
            body.LabelSet = label;
        }

        public override NodeCollection ChildNodes => new(Label, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitLabeledStatement(this);
        }

        public LabeledStatement UpdateWith(Identifier label, Statement body)
        {
            if (label == Label && body == Body)
            {
                return this;
            }

            return new LabeledStatement(label, body).SetAdditionalInfo(this);
        }
    }
}
