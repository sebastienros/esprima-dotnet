using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class DoWhileStatement : Statement
    {
        public DoWhileStatement(Statement body, Expression test) : base(Nodes.DoWhileStatement)
        {
            Body = body;
            Test = test;
        }

        public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Body, Test);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitDoWhileStatement(this, context);
        }

        public DoWhileStatement UpdateWith(Statement body, Expression test)
        {
            if (body == Body && test == Test)
            {
                return this;
            }

            return new DoWhileStatement(body, test);
        }
    }
}
