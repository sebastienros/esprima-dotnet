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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitWhileStatement(this) as T;
        }
    }
}
