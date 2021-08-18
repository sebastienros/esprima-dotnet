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

        public override NodeCollection ChildNodes => new NodeCollection(Object, Body);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitWithStatement(this);
    }
}