using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class DoWhileStatement : Statement
    {
        public readonly Statement Body;
        public readonly Expression Test;

        public DoWhileStatement(Statement body, Expression test) : base(Nodes.DoWhileStatement)
        {
            Body = body;
            Test = test;
        }

        public override NodeCollection ChildNodes => new(Body, Test);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitDoWhileStatement(this);
        }
    }
}
