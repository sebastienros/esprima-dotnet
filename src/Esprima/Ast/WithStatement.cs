using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class WithStatement : Statement
    {
        public readonly Expression Object;
        public readonly Statement Body;

        public WithStatement(Expression obj, Statement body) : base(Nodes.WithStatement)
        {
            Object = obj;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Object, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitWithStatement(this);
        }

        public WithStatement UpdateWith(Expression obj, Statement body)
        {
            if (obj == Object && body == Body)
            {
                return this;
            }

            return new WithStatement(obj, body);
        }
    }
}
