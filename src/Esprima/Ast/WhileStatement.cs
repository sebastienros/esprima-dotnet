using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class WhileStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Body;

        public WhileStatement(Expression test, Statement body) : base(Nodes.WhileStatement)
        {
            Test = test;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Test, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitWhileStatement(this);
        }

        public WhileStatement UpdateWith(Expression test, Statement body)
        {
            if (test == Test && body == Body)
            {
                return this;
            }

            return new WhileStatement(test, body).SetAdditionalInfo(this);
        }
    }
}
