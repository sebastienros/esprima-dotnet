using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class IfStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Consequent;
        public readonly Statement? Alternate;

        public IfStatement(
            Expression test,
            Statement consequent,
            Statement? alternate)
            : base(Nodes.IfStatement)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        public override NodeCollection ChildNodes => new(Test, Consequent, Alternate);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitIfStatement(this);
        }

        public IfStatement UpdateWith(Expression test, Statement consequent, Statement? alternate)
        {
            if (test == Test && consequent == Consequent && alternate == Alternate)
            {
                return this;
            }

            return new IfStatement(test, consequent, alternate);
        }
    }
}
