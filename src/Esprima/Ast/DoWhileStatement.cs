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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitDoWhileStatement(this) as T;
        }
    }
}
