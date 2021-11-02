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

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitLabeledStatement(this);
        }
    }
}
